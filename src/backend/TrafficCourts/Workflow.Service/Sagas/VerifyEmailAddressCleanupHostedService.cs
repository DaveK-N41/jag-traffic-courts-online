﻿
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TrafficCourts.Messaging.MessageContracts;

namespace TrafficCourts.Workflow.Service.Sagas;

public partial class VerifyEmailAddressCleanupHostedService : IHostedService
{
    private readonly VerifyEmailAddressStateDbContext _context;
    private readonly IBus _bus;
    private readonly ILogger<VerifyEmailAddressCleanupHostedService> _logger;

    public VerifyEmailAddressCleanupHostedService(VerifyEmailAddressStateDbContext context, IBus bus, ILogger<VerifyEmailAddressCleanupHostedService> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Starting();

        try
        {
            var states = await _context
                .Set<VerifyEmailAddressState>()
                .AsNoTracking()
                .Where(_ => _.VerifiedAt != null && _.TicketNumber != null && _.EmailAddress != null)
                .OrderBy(_ => _.VerifiedAt)
                .ToListAsync(cancellationToken);

            foreach (var state in states)
            {
                await _bus.Publish(new EmailVerificationSuccessful
                {
                    NoticeOfDisputeGuid = state.NoticeOfDisputeGuid,
                    TicketNumber = state.TicketNumber!,
                    EmailAddress = state.EmailAddress!,
                    VerifiedAt = state.VerifiedAt!.Value,
                    IsUpdateEmailVerification = state.IsUpdateEmailVerification
                }, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            Failed(exception);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Stopping();
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Starting")]
    private partial void Starting();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Stopping")]
    private partial void Stopping();

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing processing verified email addresses failed")]
    private partial void Failed(Exception exception);

}
