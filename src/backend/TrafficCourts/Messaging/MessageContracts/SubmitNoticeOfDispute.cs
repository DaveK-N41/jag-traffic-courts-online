﻿using System.Text.Json.Serialization;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;

namespace TrafficCourts.Messaging.MessageContracts;

public class SubmitNoticeOfDispute
{
    /// <summary>
    /// The unique system generated notice of dispute identifer.
    /// </summary>
    public Guid NoticeOfDisputeGuid { get; set; }

    /// <summary>
    /// The status of the dispute. Defaults to <see cref="DisputeStatus.NEW"/>.
    /// </summary>
    public DisputeStatus Status { get; set; } = DisputeStatus.NEW;

    /// <summary>
    /// The violation ticket number.
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// The date and time the violation ticket was issue. Time must only be hours and minutes.
    /// </summary>
    public DateTime IssuedTs { get; set; }

    /// <summary>
    /// The date the disputant submitted the dispute.
    /// </summary>
    public DateTime? SubmittedTs { get; set; }

    /// <summary>
    /// The surname or corporate name.
    /// </summary>
    public string? ContactSurnameNm { get; set; }

    /// <summary>
    /// The given names or corporate name continued.
    /// </summary>
    public string? ContactGiven1Nm { get; set; }

    /// <summary>
    /// The given names or corporate name continued.
    /// </summary>
    public string? ContactGiven2Nm { get; set; }

    /// <summary>
    /// The given names or corporate name continued.
    /// </summary>
    public string? ContactGiven3Nm { get; set; }

    /// <summary>
    /// The surname or corporate name.
    /// </summary>
    public string? DisputantSurname { get; set; }

    /// <summary>
    /// The given names or corporate name continued.
    /// </summary>
    public string? DisputantGivenName1 { get; set; }

    /// <summary>
    /// The given names or corporate name continued.
    /// </summary>
    public string? DisputantGivenName2 { get; set; }

    /// <summary>
    /// The given names or corporate name continued.
    /// </summary>
    public string? DisputantGivenName3 { get; set; }

    /// <summary>
    /// Contact Law Firm Name.
    /// </summary>
    public string? ContactLawFirmNm { get; set; }

    /// <summary>
    /// Contact Type.
    /// </summary>
    public DisputeContactTypeCd ContactTypeCd { get; set; }

    /// <summary>
    /// The disputant's birthdate.
    /// </summary>
    public DateTime DisputantBirthdate { get; set; }

    /// <summary>
    /// The drivers licence number. Note not all jurisdictions will use numeric drivers licence numbers.
    /// </summary>
    public string? DriversLicenceNumber { get; set; }

    /// <summary>
    /// The province or state the drivers licence was issued by.
    /// </summary>
    public string? DriversLicenceProvince { get; set; }

    /// <summary>
    /// The province sequence number of the drivers licence was issued by.
    /// </summary>
    public int? DriversLicenceProvinceSeqNo { get; set; }

    /// <summary>
    /// The country code of the drivers licence was issued by.
    /// </summary>
    public int? DriversLicenceCountryId { get; set; }

    /// <summary>
    /// The mailing address line one of the disputant.
    /// </summary>
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// The mailing address line two of the disputant.
    /// </summary>
    public string? AddressLine2 { get; set; }
    /// <summary>
    /// The mailing address line three of the disputant.
    /// </summary>
    public string? AddressLine3 { get; set; }

    /// <summary>
    /// The mailing address city of the disputant.
    /// </summary>
    public string? AddressCity { get; set; }

    /// <summary>
    /// The mailing address province of the disputant.
    /// </summary>
    public string? AddressProvince { get; set; }

    /// <summary>
    /// The mailing address province's country code of the disputant.
    /// </summary>
    public int? AddressProvinceCountryId { get; set; }

    /// <summary>
    /// The mailing address province's sequence number of the disputant.
    /// </summary>
    public int? AddressProvinceSeqNo { get; set; }

    /// <summary>
    /// The mailing address country id of the disputant.
    /// </summary>
    public int? AddressCountryId { get; set; }

    /// <summary>
    /// The mailing address postal code or zip code of the disputant.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// The disputant's home phone number.
    /// </summary>
    public string? HomePhoneNumber { get; set; }

    /// <summary>
    /// The disputant's work phone number.
    /// </summary>
    public string? WorkPhoneNumber { get; set; }

    /// <summary>
    /// The disputant's email address.
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// The disputant intends to be represented by a lawyer at the hearing.
    /// </summary>
    public DisputeRepresentedByLawyer RepresentedByLawyer { get; set; }

    /// <summary>
    /// Name of the law firm that will represent the disputant at the hearing.
    /// </summary>
    public string LawFirmName { get; set; } = String.Empty;

    /// <summary>
    /// Surname of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    public string LawyerSurname { get; set; } = String.Empty;

    /// <summary>
    /// Given name1 of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    public string LawyerGivenName1 { get; set; } = String.Empty;

    /// <summary>
    /// Given name2 of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    public string LawyerGivenName2 { get; set; } = String.Empty;

    /// <summary>
    /// Given name3 of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    public string LawyerGivenName3 { get; set; } = String.Empty;

    /// <summary>
    /// Email address of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    public string LawyerEmail { get; set; } = String.Empty;

    /// <summary>
    /// Address of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    public string LawyerAddress { get; set; } = String.Empty;

    /// <summary>
    /// Address of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    public string LawyerPhoneNumber { get; set; } = String.Empty;

    /// <summary>
    /// Detachment Location
    /// </summary>
    public string? DetachmentLocation { get; set; }

    /// <summary>
    /// Court Agency Id
    /// </summary>
    public string? CourtAgenId { get; set; }

    /// <summary>
    /// The disputant requires spoken language interpreter. The language name is indicated in this field.
    /// </summary>
    public string? InterpreterLanguageCd { get; set; }

    /// <summary>
    /// Interpreter required?
    /// </summary>
    public DisputeInterpreterRequired InterpreterRequired { get; set; } = DisputeInterpreterRequired.N;

    /// <summary>
    /// The number of witnesses that the disputant intends to call.
    /// </summary>
    public int WitnessNo { get; set; }

    /// <summary>
    /// The reason that disputant declares for requesting a fine reduction.
    /// </summary>
    public string? FineReductionReason { get; set; }

    /// <summary>
    /// The reason that disputant declares for requesting more time to pay the amount on the violation ticket.
    /// </summary>
    public string? TimeToPayReason { get; set; }

    /// <summary>
    /// Identifier for whether the citizen has detected any issues with the OCR ticket result or not.
    /// </summary>
    public DisputeDisputantDetectedOcrIssues DisputantDetectedOcrIssues { get; set; }

    /// <summary>
    /// Did the disputant request a court appearance?
    /// </summary>
    public DisputeRequestCourtAppearanceYn RequestCourtAppearanceYn { get; set; }

    /// <summary>
    /// Identifier for whether the citizen has detected any issues with the OCR ticket result or not.
    /// </summary>
    public DisputeSystemDetectedOcrIssues SystemDetectedOcrIssues { get; set; }

    /// <summary>
    /// Is court appearance less than 14 days
    /// </summary>
    public DisputeAppearanceLessThan14DaysYn AppearanceLessThan14DaysYn { get; set; }

    /// <summary>
    /// The description of the issue with OCR ticket if the citizen has detected any.
    /// </summary>
    public string? DisputantOcrIssues { get; set; }

    /// <summary>
    /// Name of the person who signed the dispute.
    /// </summary>
    public string? SignatoryName { get; set; }

    /// <summary>
    /// Signatory Type. Can be either 'D' for Disputant or 'A' for Agent.
    /// </summary>
    public DisputeSignatoryType? SignatoryType { get; set; }

    /// <summary>
    /// Violation Ticket details
    /// </summary>
    public ViolationTicket? ViolationTicket { get; set; }

    /// <summary>
    /// Filename of JSON serialized OCR data that is saved in object storage.
    /// </summary>
    [Obsolete("OCR Results save with properties notice of dispute id and document type = 'OCR Result'")]
    public string? OcrTicketFilename { get; set; }

    public IList<DisputeCount> DisputeCounts { get; set; } = new List<DisputeCount>();
}
