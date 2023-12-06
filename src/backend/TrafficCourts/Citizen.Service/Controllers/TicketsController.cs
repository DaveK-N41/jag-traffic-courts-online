﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using TrafficCourts.Citizen.Service.Features.Tickets;
using TrafficCourts.Citizen.Service.Services.Tickets.Search;
using TrafficCourts.Citizen.Service.Validators;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;

namespace TrafficCourts.Citizen.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TicketsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TicketsController> _logger;
        private readonly ITicketSearchService _ticketSearchService;

        public TicketsController(IMediator mediator, ITicketSearchService ticketSearchService, ILogger<TicketsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _ticketSearchService = ticketSearchService ?? throw new ArgumentNullException(nameof(ticketSearchService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Searches for a violation ticket that exists on file.
        /// </summary>
        /// <param name="ticketNumber">The violation ticket number. Must start with two upper case letters and end with eight digits.</param>
        /// <param name="time">The time the violation ticket number was issued. Must be formatted a valid 24-hour clock, HH:MM.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <response code="200">The violation ticket was found.</response>
        /// <response code="400">The request was not well formed. Check the parameters.</response>
        /// <response code="404">The violation ticket was not found.</response>
        /// <response code="500">There was a server error that prevented the search from completing successfully.</response>
        [HttpGet]
        [ProducesResponseType(typeof(Models.Tickets.ViolationTicket), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchAsync(
            [FromQuery]
            [Required]
            [RegularExpression(Search.Request.TicketNumberRegex, ErrorMessage = "ticketNumber must start with two upper case letters and 6 or more numbers")] string ticketNumber,
            [FromQuery]
            [Required]
            [RegularExpression(Search.Request.TimeRegex, ErrorMessage = "time must be properly formatted 24 hour clock")] string time,
            CancellationToken cancellationToken)
        {
            Search.Request request = new(ticketNumber, time);
            Search.Response response = await _mediator.Send(request, cancellationToken);

            if (response == Search.Response.Empty)
            {
                return NotFound();
            }

            try
            {
                var check = await _ticketSearchService.IsDisputeSubmittedBefore(ticketNumber, cancellationToken);
                if (check)
                {
                    return BadRequest($"A dispute has already been submitted for the ticket number: {ticketNumber}. A dispute can only be submitted once for a violation ticket.");
                }
            }
            catch (DisputeSearchFailedException e)
            {
                ProblemDetails problemDetails = new();
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = e.Source + ": Error searching dispute for the ticket";
                problemDetails.Instance = HttpContext?.Request?.Path;
                string? innerExceptionMessage = e.InnerException?.Message;
                if (innerExceptionMessage is not null)
                {
                    problemDetails.Extensions.Add("errors", new string[] { e.Message, innerExceptionMessage });
                }
                else
                {
                    problemDetails.Extensions.Add("errors", new string[] { e.Message });
                }

                return new ObjectResult(problemDetails);
            }

            var result = response.Result.Match<IActionResult>(
                ticket => Ok(ticket),
                exception => StatusCode(StatusCodes.Status500InternalServerError),
                invalidTicketException => StatusCode(StatusCodes.Status400BadRequest));

            return result;
        }

        /// <summary>
        /// Analyses a Traffic Violation Ticket, extracting all hand-written text to a consumable JSON object.
        /// </summary>
        /// <param name="file">A PNG, JPEG, or PDF of a scanned Traffic Violation Ticket</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="sanitize">If true, will perform basic cleanup on ocr results before validation. Defaults to true.</param>
        /// <param name="validate">If true, will perform business validation on ocr results. Defaults to true.</param>
        /// <returns></returns>
        /// <response code="200">The file appears to be a valid Violation Ticket. JSON data is extracted.</response>
        /// <response code="400">The uploaded file is too large or the Violation Ticket does not appear to be valid. Either 
        /// the ticket title could not be found, the ticket number is invalid, the violation date is invalid or more than 
        /// 30 days ago, or MVA is not selected or not the only ACT selected.</response>
        /// <response code="500">There was a server error that prevented the analyse from completing successfully.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OcrViolationTicket), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [RequestSizeLimit(10485760)]
        public async Task<IActionResult> AnalyseAsync(
            [Required]
            [PermittedFileContentType(new string[] { "image/png", "image/jpeg", "application/pdf", "application/octet-stream" })] 
            IFormFile file,
            CancellationToken cancellationToken,
            bool sanitize = true,
            bool validate = true)
        {
            AnalyseHandler.AnalyseRequest request = new(file);
            request.Sanitize = sanitize;
            request.Validate = validate;
            AnalyseHandler.AnalyseResponse response;
            try
            {
                response = await _mediator.Send(request, cancellationToken);
            }
            catch (Azure.RequestFailedException e)
            {
                _logger.LogError(e, "Azure.RequestFailedException");
                ProblemDetails problemDetails = new();
                problemDetails.Status = e.Status;
                problemDetails.Title = e.Source + ": " + e.ErrorCode;
                problemDetails.Instance = HttpContext?.Request?.Path;
                problemDetails.Extensions.Add("errors", e.Message);

                return new ObjectResult(problemDetails);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception invoking Azure Form Recognizer");
                ProblemDetails problemDetails = new();
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Error invoking Azure Form Recognizer";
                problemDetails.Instance = HttpContext?.Request?.Path;
                string? innerExceptionMessage = e.InnerException?.Message;
                if (innerExceptionMessage is not null)
                {
                    problemDetails.Extensions.Add("errors", new string[] { e.Message, innerExceptionMessage });
                }
                else
                {
                    problemDetails.Extensions.Add("errors", new string[] { e.Message });
                }

                return new ObjectResult(problemDetails);
            }

            if (response.OcrViolationTicket.GlobalValidationErrors.Count > 0)
            {
                // Return BadRequest 
                // - if the file is not an image/pdf of a TrafficViolation (could not read title)
                // - if the TicketNumber could not be extracted or is invalid (ie doesn't start with an A)
                // - if MVA is not the only checkbox selected under the 'Did commit the offence(s) indicated' section
                // - if ViolationDate is > 30 days ago
                ProblemDetails problemDetails = new();
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Violation Ticket is not valid or could not be read.";
                problemDetails.Instance = HttpContext?.Request?.Path;
                problemDetails.Extensions.Add("errors", response.OcrViolationTicket.GlobalValidationErrors);

                return new ObjectResult(problemDetails);
            }

            string? ticketNumber = response.OcrViolationTicket.Fields[OcrViolationTicket.ViolationTicketNumber].Value;
            // Verify a dispute for the ticket has not been submitted before
            try
            {
                var check = await _ticketSearchService.IsDisputeSubmittedBefore(ticketNumber!, cancellationToken);
                if (check)
                {
                    return BadRequest($"A dispute has already been submitted for the ticket number: {ticketNumber}. A dispute can only be submitted once for a violation ticket.");
                }
            }
            catch (DisputeSearchFailedException e)
            {
                ProblemDetails problemDetails = new();
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = e.Source + ": Error searching dispute for the ticket";
                problemDetails.Instance = HttpContext?.Request?.Path;
                string? innerExceptionMessage = e.InnerException?.Message;
                if (innerExceptionMessage is not null)
                {
                    problemDetails.Extensions.Add("errors", new string[] { e.Message, innerExceptionMessage });
                }
                else
                {
                    problemDetails.Extensions.Add("errors", new string[] { e.Message });
                }

                return new ObjectResult(problemDetails);
            }

            return Ok(response.OcrViolationTicket);
        }
    }
}
