namespace TrafficCourts.Domain.Models;

/// <summary>
/// Represents a summary of a dispute case file. This is a subset of the information available for a dispute case file.
/// A dispute case file is also known as a TCO Dispute or JJ Dispute.
/// </summary>
public class DisputeCaseFileSummary
{
    public long Id { get; set; }
    public DateTimeOffset? SubmittedTs { get; set; }
    public System.DateTimeOffset? JjDecisionDate { get; set; }
    public string SignatoryName { get; set; }
    public JJDisputeHearingType? HearingType { get; set; }
    public string TicketNumber { get; set; }
    public DateTimeOffset? ViolationDate { get; set; }
    public int ViolationDateCount { get; set; }
    public decimal? ToBeHeardAtCourthouseId { get; set; }
    public string ToBeHeardAtCourthouseName { get; set; }
    public string DisputantSurname { get; set; }
    public string DisputantGivenName1 { get; set; }
    public string DisputantGivenName2 { get; set; }
    public string DisputantGivenName3 { get; set; }
    
    public JJDisputeStatus Status { get; set; }
    public string DisputeStatusCd { get; set; }
    public string DisputeStatusDescription { get; set; }

    public string StatusDescription { get; set; }
    public string PoliceDetachment { get; set; }
    public decimal? PoliceDetachmentId { get; set; }
    public YesNo? AccidentYn { get; set; }
    public YesNo? MultipleOfficersYn { get; set; }
    public YesNo? NoticeOfHearingYn { get; set; }
    public YesNo? ElectronicTicketYn { get; set; }

    public string JjAssignedTo { get; set; }
    public string VtcAssignedTo { get; set; }
    public DateTimeOffset? VtcAssignedTs { get; set; }

    public decimal? AppearanceCourthouseId { get; set; }
    public string? AppearanceCourthouseName { get; set; }
    public string? AppearanceRoomCode { get; set; }
    public DateTimeOffset? AppearanceTs { get; set; }
}
