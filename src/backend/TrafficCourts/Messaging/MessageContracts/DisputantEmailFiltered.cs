﻿using TrafficCourts.Messaging.Models;

namespace TrafficCourts.Messaging.MessageContracts
{
    /// <summary>
    /// Raised when an email was filtered before being sent due to allow list processing.
    /// </summary>
    public class DisputantEmailFiltered
    {
        /// <summary>
        /// The email message.
        /// </summary>
        public EmailMessage? Message { get; set; }


        /// <summary>
        /// The date and time the message was processed and filtered.
        /// </summary>
        public DateTimeOffset FilteredAt { get; set; }

        /// <summary>
        /// occam dispute id
        /// </summary>
        public long OccamDisputeId { get; set; }    
    }
}
