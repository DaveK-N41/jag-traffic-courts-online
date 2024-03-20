﻿using TrafficCourts.Domain.Models;
using TrafficCourts.Messaging.MessageContracts;
namespace TrafficCourts.Workflow.Service.Mappers;

public class Mapper
{
    public static EmailHistory ToEmailHistory(DisputantEmailSent src)
    {
        ArgumentNullException.ThrowIfNull(src);

        EmailHistory target = new EmailHistory();

        target.EmailSentTs = src.SentAt;
        target.ToEmailAddress = src.Message?.To is not null ? src.Message.To : "unknown";
        target.SuccessfullySent = EmailHistorySuccessfullySent.Y;
        target.Subject = src.Message?.Subject is not null ? src.Message.Subject : "unknown";
        target.HtmlContent = src.Message?.HtmlContent;
        target.PlainTextContent = src.Message?.TextContent;
        target.OccamDisputeId = src.OccamDisputeId;
        target.FromEmailAddress = src.Message?.From is not null ? src.Message.From : "unknown";
        
        return target;
    }

    public static EmailHistory ToEmailHistory(DisputantEmailFiltered src)
    {
        ArgumentNullException.ThrowIfNull(src);

        EmailHistory target = new EmailHistory();

        target.EmailSentTs = src.FilteredAt;
        target.ToEmailAddress = src.Message?.To is not null ? src.Message.To : "unknown";
        target.SuccessfullySent = EmailHistorySuccessfullySent.N;
        target.Subject = src.Message?.Subject is not null ? src.Message.Subject : "unknown";

        // ensure only one of html content or plain text content is set
        target.HtmlContent = src.Message?.HtmlContent;
        target.PlainTextContent = src.Message?.TextContent;
        target.OccamDisputeId = src.OccamDisputeId;
        target.FromEmailAddress = src.Message?.From is not null ? src.Message.From : "unknown";

        return target;
    }
}
