﻿using AutoMapper;
using MassTransit;
using TrafficCourts.Domain.Models;
using TrafficCourts.Interfaces;
using TrafficCourts.Messaging.MessageContracts;

namespace TrafficCourts.Workflow.Service.Consumers
{
    /// <summary>
    /// Creates the dispute in the database.
    /// </summary>
    public class CreateDisputeInDatabaseConsumer : IConsumer<SubmitNoticeOfDispute>
    {
        private readonly ILogger<CreateDisputeInDatabaseConsumer> _logger;
        private readonly IOracleDataApiService _oracleDataApiService;
        private readonly IMapper _mapper;

        public CreateDisputeInDatabaseConsumer(ILogger<CreateDisputeInDatabaseConsumer> logger, IOracleDataApiService oracleDataApiService, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _oracleDataApiService = oracleDataApiService ?? throw new ArgumentNullException(nameof(oracleDataApiService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task Consume(ConsumeContext<SubmitNoticeOfDispute> context)
        {
            using var loggingScope = _logger.BeginConsumeScope(context, message => message.NoticeOfDisputeGuid);

            try
            {
                _logger.LogDebug("Consuming message");
                var cancellationToken = context.CancellationToken;

                Dispute dispute = _mapper.Map<Dispute>(context.Message);

                _logger.LogTrace("TRY CREATING DISPUTE: {@Dispute}", dispute);

                var disputeId = await _oracleDataApiService.SaveDisputeAsync(dispute, cancellationToken);

                if (disputeId > 0)
                {
                    _logger.LogDebug("Dispute has been saved with {DisputeId}: ", disputeId);

                    await context.PublishWithLog(_logger, new NoticeOfDisputeSubmitted
                    {
                        NoticeOfDisputeGuid = context.Message.NoticeOfDisputeGuid,
                    }, cancellationToken);
                }
                else
                {
                    _logger.LogDebug("Failed to save the dispute");

                    // TODO handle this better
                    // SubmitNoticeOfDisputeFailed
                    await context.Publish<DisputeRejected>(new
                    {
                        Reason = "Bad request"
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message");
                throw;
            }
        }
    }
}
