﻿
using System.Text;
using System.Text.Json.Serialization;

namespace TrafficCourts.Workflow.Service.Models
{
    public class NoticeOfDispute
    {
        public DisputeStatus Status { get; set; }
        public string? TicketNumber { get; set; }
        public string? ProvincialCourtHearingLocation { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string? Surname { get; set; }
        public string? GivenNames { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? DriversLicenceNumber { get; set; }
        public string? DriversLicenceProvince { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? HomePhoneNumber { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public string? EmailAddress { get; set; }
        public List<DisputedCount> DisputedCounts { get; set; } = new List<DisputedCount>();
        public bool RepresentedByLawyer { get; set; }
        public LegalRepresentation? LegalRepresentation { get; set; }
        public string? InterpreterLanguage { get; set; }
        public int NumberOfWitness { get; set; }
        public string? FineReductionReason { get; set; }
        public string? TimeToPayReason { get; set; }
        public bool DisputantDetectedOcrIssues { get; set; }
        public string? DisputantOcrIssuesDescription { get; set; }
        public ViolationTicket? ViolationTicket { get; set; }
        public string? OcrViolationTicket { get; set; }

        public override string ToString()
        {
            return GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(this, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
        }
    }

    public class DisputedCount
    {
        public Plea Plea { get; set; }
        public int Count { get; set; }
        public bool RequestTimeToPay { get; set; }
        public bool RequestReduction { get; set; }
        public bool AppearInCourt { get; set; }
    }

    public class LegalRepresentation
    {
        public string LawFirmName { get; set; } = String.Empty;
        public string LawyerFullName { get; set; } = String.Empty;
        public string LawyerEmail { get; set; } = String.Empty;
        public string LawyerAddress { get; set; } = String.Empty;
        public string LawyerPhoneNumber { get; set; } = String.Empty;
    }

    /// <summary>
    /// An enumeration of Plea Type on a DisputedCount record.
    /// </summary>
    public enum Plea
    {
        /// <summary>
        /// If the dispuant is pleads guilty, plea will always be Guilty. The dispuant has choice to attend court or not.
        /// </summary>
        Guily,

        /// <summary>
        /// If the dispuant is pleads not guilty, the dispuant will have to attend court.
        /// </summary>
        NotGuilty
    }

    /// <summary>
    /// An enumeration of available Statuses on a Dispute record.
    /// </summary>
    public enum DisputeStatus
    {
        New,
        Processing,
        Rejected,
        Cancelled
    }

}
