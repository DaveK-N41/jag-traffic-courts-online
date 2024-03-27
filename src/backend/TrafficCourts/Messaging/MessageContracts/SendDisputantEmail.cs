﻿using TrafficCourts.Messaging.Models;

namespace TrafficCourts.Messaging.MessageContracts
{
    /// <summary>
    /// Raised when an email should be sent to a disputant.
    /// </summary>
    public class SendDisputantEmail
    {
        /// <summary>
        /// The email message.
        /// </summary>
        public EmailMessage Message { get; set; } = new();
        
        /// <summary>
        /// The ticket number.
        /// </summary>
        public string? TicketNumber { get; set; } = String.Empty;

        /// <summary>
        /// The notice of dispute identifer.
        /// </summary>
        public Guid NoticeOfDisputeGuid { get; set; }
    }
}
