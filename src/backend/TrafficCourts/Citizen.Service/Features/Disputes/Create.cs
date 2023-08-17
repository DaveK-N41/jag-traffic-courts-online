using MassTransit;
using MediatR;
using TrafficCourts.Citizen.Service.Models.Disputes;
using TrafficCourts.Citizen.Service.Services;
using TrafficCourts.Messaging.MessageContracts;
using AutoMapper;
using System.Diagnostics;
using NodaTime;
using HashidsNet;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;
using TrafficCourts.Coms.Client;
using TrafficCourts.Common.Models;
using System.Text.Json;

namespace TrafficCourts.Citizen.Service.Features.Disputes
{
    public static class Create
    {
        public class Request : IRequest<Response>
        {
            public NoticeOfDispute Dispute { get; init; }

            public Request(NoticeOfDispute dispute)
            {
                Dispute = dispute ?? throw new ArgumentNullException(nameof(dispute));
            }
        }
        public class Response
        {
            /// <summary>
            /// Creates a successful response.
            /// </summary>
            public Response()
            {
            }

            public Response(Exception exception)
            {
                Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            }

            public Response(string noticeOfDisputeGuid)
            {
                NoticeOfDisputeGuid = EmailVerificationToken = noticeOfDisputeGuid;
            }

            public Exception? Exception { get; init; }
            public string? EmailVerificationToken { get; }
            public string? NoticeOfDisputeGuid { get;}
        }

        public class Handler : IRequestHandler<Request, Response>
        {
            private readonly ILogger _logger;
            private readonly IBus _bus;
            private readonly IRedisCacheService _redisCacheService;
            private readonly IMapper _mapper;
            private readonly IClock _clock;
            private readonly IHashids _hashids;
            private readonly IObjectManagementService _objectManagementService;
            private readonly IMemoryStreamManager _memoryStreamManager;

            /// <summary>
            /// Creates the handler.
            /// </summary>
            /// <param name="bus"></param>
            /// <param name="redisCacheService"></param>
            /// <param name="objectManagementService"></param>
            /// <param name="memoryStreamManager"></param>
            /// <param name="mapper"></param>
            /// <param name="clock"></param>
            /// <param name="hashids"></param>
            /// <param name="logger"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public Handler(
                IBus bus, 
                IRedisCacheService redisCacheService,
                IObjectManagementService objectManagementService,
                IMemoryStreamManager memoryStreamManager,
                IMapper mapper, 
                IClock clock,
                IHashids hashids,
                ILogger<Handler> logger)
            {
                _bus = bus ?? throw new ArgumentNullException(nameof(bus));
                _redisCacheService = redisCacheService ?? throw new ArgumentNullException(nameof(redisCacheService));
                _objectManagementService = objectManagementService ?? throw new ArgumentNullException(nameof(objectManagementService));
                _memoryStreamManager = memoryStreamManager ?? throw new ArgumentNullException(nameof(memoryStreamManager));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
                _clock = clock ?? throw new ArgumentNullException(nameof(clock));
                _hashids = hashids ?? throw new ArgumentNullException(nameof(hashids));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(request);

                using Activity? activity = Diagnostics.Source.StartActivity("Create Dispute");

                NoticeOfDispute dispute = request.Dispute;
                string? ticketId = dispute.TicketId;
                OcrViolationTicket? violationTicket = null;
                Models.Tickets.ViolationTicket? lookedUpViolationTicket = null;
                MemoryStream? ticketImageStream = null;

                // create the noticeOfDisputeId
                Guid noticeOfDisputeId = NewId.NextSequentialGuid();

                try
                {
                    // Check if the request contains ticket id
                    if (!String.IsNullOrEmpty(ticketId))
                    {
                        // Check if the ticket id belongs to an OCR type of ticket
                        if (ticketId.EndsWith("-o"))
                        {
                            // Get the OCR violation ticket data from Redis cache using the ticket id key
                            violationTicket = await _redisCacheService.GetRecordAsync<OcrViolationTicket>(ticketId);

                            if (violationTicket is null || String.IsNullOrEmpty(violationTicket.ImageFilename))
                            {
                                _logger.LogInformation("No OCR violation ticket and image filename have been found for the {TicketId}", ticketId);
                                throw new TicketLookupFailedException($"No OCR violation ticket and image filename has been found for the {ticketId}");
                            }

                            // Since the ImageFilename exists, it should be in the redis db.
                            // grab it and save to the file persistence service.
                            ticketImageStream = await _redisCacheService.GetFileRecordAsync(violationTicket.ImageFilename);

                            if (ticketImageStream is not null)
                            {
                                Guid id = await SaveTicketImageAsync(noticeOfDisputeId, ticketImageStream, violationTicket.ImageFilename, cancellationToken);

                                // remove the image from the redis cache, to free-up the space.
                                await _redisCacheService.DeleteRecordAsync(violationTicket.ImageFilename);
                            }
                        }
                        // Check if the ticket id belongs to a Looked Up type of ticket
                        else if (ticketId.EndsWith("-l"))
                        {
                            // Get the looked up violation ticket data from Redis cache using the ticket id key
                            lookedUpViolationTicket = await _redisCacheService.GetRecordAsync<Models.Tickets.ViolationTicket>(ticketId);

                            if (lookedUpViolationTicket is null)
                            {
                                _logger.LogInformation("No looked up violation ticket has been found for the {TicketId}", ticketId);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("An invalid {TicketId} has been passed", ticketId);
                        }
                    }

                    if (violationTicket == null && lookedUpViolationTicket == null)
                    {
                        Exception ex = new TicketLookupFailedException("No associated Violation Ticket has been found");
                        _logger.LogError(ex, "Error creating dispute - No associated Violation Ticket has been found");
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        return new Response(ex);
                    }

                    SubmitNoticeOfDispute submitNoticeOfDispute = _mapper.Map<SubmitNoticeOfDispute>(dispute);
                    
                    submitNoticeOfDispute.AppearanceLessThan14DaysYn = dispute.AppearanceLessThan14Days is true 
                        ? DisputeAppearanceLessThan14DaysYn.Y 
                        : DisputeAppearanceLessThan14DaysYn.N;

                    submitNoticeOfDispute.NoticeOfDisputeGuid = noticeOfDisputeId;
                    submitNoticeOfDispute.SubmittedTs = _clock.GetCurrentInstant().ToDateTimeUtc();

                    if (violationTicket != null)
                    {
                        var id = await SaveTicketOcrResultAsync(noticeOfDisputeId, violationTicket, cancellationToken);
                        // OcrTicketFilename is obsolete, however, this field cannot be empty otherwise violation ticket will not be initialized
                        submitNoticeOfDispute.OcrTicketFilename = id.ToString("d"); 
                    }

                    if (lookedUpViolationTicket != null)
                    {
                        submitNoticeOfDispute.ViolationTicket = _mapper.Map<Messaging.MessageContracts.ViolationTicket>(lookedUpViolationTicket);
                    }

                    // Publish submit NoticeOfDispute event (consumer(s) will push event to Oracle Data API to save the Dispute and generate email)
                    await _bus.PublishWithLog(_logger, submitNoticeOfDispute, cancellationToken);

                    // success, return true
                    activity?.SetStatus(ActivityStatusCode.Ok);

                    var hash = _hashids.EncodeHex(submitNoticeOfDispute.NoticeOfDisputeGuid.ToString("n"));
                    return new Response(hash);
                }
                catch (Exception exception)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                    _logger.LogError(exception, "Error creating dispute");
                    return new Response(exception);
                }
            }

            /// <summary>
            /// Saves the ticket image associated with Notice of Dispute
            /// </summary>
            private async Task<Guid> SaveTicketImageAsync(Guid noticeOfDisputeId, MemoryStream stream, string filename, CancellationToken cancellationToken)
            {
                InternalFileProperties properties = new InternalFileProperties
                { 
                    NoticeOfDisputeId = noticeOfDisputeId, 
                    DocumentType = InternalFileProperties.DocumentTypes.TicketImage
                };

                var metadata = properties.ToMetadata();
                var tags = properties.ToTags();

                Coms.Client.File file = new Coms.Client.File(stream, filename, null, metadata, tags);
                Guid id = await _objectManagementService.CreateFileAsync(file, cancellationToken);

                // Publish a message to virus scan the newly uploaded file
                await _bus.PublishWithLog(_logger, new DocumentUploaded { Id = id }, cancellationToken);

                return id;
            }

            /// <summary>
            /// Saves the OCR result associated with Notice of Dispute
            /// </summary>
            private async Task<Guid> SaveTicketOcrResultAsync(Guid noticeOfDisputeId, OcrViolationTicket violationTicket, CancellationToken cancellationToken)
            {
                InternalFileProperties properties = new InternalFileProperties
                { 
                    NoticeOfDisputeId = noticeOfDisputeId, 
                    DocumentType = InternalFileProperties.DocumentTypes.OcrResult
                };

                var metadata = properties.ToMetadata();
                var tags = properties.ToTags();

                using var stream = _memoryStreamManager.GetStream();
                JsonSerializer.Serialize(stream, violationTicket);
                stream.Position = 0L;

                Coms.Client.File file = new Coms.Client.File(stream, "ocr-result.json", "application/json", metadata, tags);
                Guid id = await _objectManagementService.CreateFileAsync(file, cancellationToken);

                // Publish a message to virus scan the newly uploaded file
                await _bus.PublishWithLog(_logger, new DocumentUploaded { Id = id }, cancellationToken);

                return id;
            }
        }
    }

    [Serializable]
    internal class TicketLookupFailedException : Exception
    {
        public TicketLookupFailedException(string? message) : base(message) { }
    }
}
