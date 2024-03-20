﻿using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;

namespace TrafficCourts.Citizen.Service.Models.Disputes;

/// <summary>
/// Represents a violation ticket notice of dispute.
/// </summary>
public class Dispute : DisputantContactInformation
{
    /// <summary>
    /// The violation ticket number.
    /// </summary>
    [JsonPropertyName("ticket_number")]
    [MaxLength(12)]
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// The date and time the violation ticket was issue. Time must only be hours and minutes.
    /// </summary>
    [JsonPropertyName("issued_date")]
    public DateTime IssuedTs { get; set; }

    /// <summary>
    /// The disputant's birthdate.
    /// </summary>
    [JsonPropertyName("disputant_birthdate")]
    [SwaggerSchema(Format = "date")]
    public DateTime DisputantBirthdate { get; set; }

    /// <summary>
    /// The drivers licence number. Note not all jurisdictions will use numeric drivers licence numbers.
    /// </summary>
    [JsonPropertyName("drivers_licence_number")]
    public string? DriversLicenceNumber { get; set; }

    /// <summary>
    /// The province or state the drivers licence was issued by.
    /// </summary>
    [JsonPropertyName("drivers_licence_province")]
    public string? DriversLicenceProvince { get; set; }

    /// <summary>
    /// The province sequence number of the drivers licence was issued by.
    /// </summary>
    [JsonPropertyName("drivers_licence_province_seq_no")]
    public int? DriversLicenceProvinceSeqNo { get; set; }

    /// <summary>
    /// The country code of the drivers licence was issued by.
    /// </summary>
    [JsonPropertyName("drivers_licence_country_id")]
    public int? DriversLicenceCountryId { get; set; }

    /// <summary>
    /// The disputant's work phone number.
    /// </summary>
    [JsonPropertyName("work_phone_number")]
    public string? WorkPhoneNumber { get; set; }

    /// <summary>
    /// The disputant intends to be represented by a lawyer at the hearing.
    /// </summary>
    [JsonPropertyName("represented_by_lawyer")]
    public DisputeRepresentedByLawyer RepresentedByLawyer { get; set; } = DisputeRepresentedByLawyer.N;

    /// <summary>
    /// Name of the law firm that will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("law_firm_name")]
    public string? LawFirmName { get; set; }

    /// <summary>
    /// Surname of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("lawyer_surname")]
    public string? LawyerSurname { get; set; }

    /// <summary>
    /// Given Name 1 of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("lawyer_given_name1")]
    public string? LawyerGivenName1 { get; set; }

    /// <summary>
    /// Given Name 2 of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("lawyer_given_name2")]
    public string? LawyerGivenName2 { get; set; }

    /// <summary>
    /// Given Name 3 of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("lawyer_given_name3")]
    public string? LawyerGivenName3 { get; set; }

    /// <summary>
    /// Email address of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("lawyer_email")]
    public string? LawyerEmail { get; set; }

    /// <summary>
    /// Address of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("lawyer_address")]
    public string? LawyerAddress { get; set; }

    /// <summary>
    /// Address of the lawyer who will represent the disputant at the hearing.
    /// </summary>
    [JsonPropertyName("lawyer_phone_number")]
    public string? LawyerPhoneNumber { get; set; }

    /// <summary>
    /// Request Court Appearance
    /// </summary>
    [JsonPropertyName("request_court_appearance")]
    public DisputeRequestCourtAppearanceYn RequestCourtAppearanceYn { get; set; }

    /// <summary>
    /// The disputant requires spoken language interpreter. The language name is indicated in this field.
    /// </summary>
    [JsonPropertyName("interpreter_language_cd")]
    public string? InterpreterLanguageCd { get; set; }

    /// <summary>
    /// Interpreter Required
    /// </summary>
    [JsonPropertyName("interpreter_required")]
    public DisputeInterpreterRequired? InterpreterRequired { get; set; } = DisputeInterpreterRequired.N;

    /// <summary>
    /// The number of witnesses that the disputant intends to call.
    /// </summary>
    [JsonPropertyName("witness_no")]
    public int WitnessNo { get; set; }

    /// <summary>
    /// The reason that disputant declares for requesting a fine reduction.
    /// </summary>
    [JsonPropertyName("fine_reduction_reason")]
    public string? FineReductionReason { get; set; }

    /// <summary>
    /// Name of the person who signed the dispute.
    /// </summary>
    [JsonPropertyName("signatory_name")]
    public string? SignatoryName { get; set; }

    /// <summary>
    /// Signatory Type. Can be either 'D' for Disputant or 'A' for Agent.
    /// </summary>
    [JsonPropertyName("signatory_type")]
    public DisputeSignatoryType? SignatoryType { get; set; }

    /// <summary>
    /// The reason that disputant declares for requesting more time to pay the amount on the violation ticket.
    /// </summary>
    [JsonPropertyName("time_to_pay_reason")]
    public string? TimeToPayReason { get; set; }

    /// <summary>
    /// Dispute Counts
    /// </summary>
    [JsonPropertyName("dispute_counts")]
    public ICollection<DisputeCount>? DisputeCounts { get; set; }

    /// <summary>
    /// List of file metadata that contain ID and Filename of all the uploaded documents related to this particular JJDispute
    /// </summary>
    /// 
    [JsonPropertyName("file_data")]
    public List<TrafficCourts.Domain.Models.FileMetadata>? FileData { get; set; }
}
