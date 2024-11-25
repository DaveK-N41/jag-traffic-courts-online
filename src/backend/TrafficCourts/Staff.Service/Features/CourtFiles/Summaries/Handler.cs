using MediatR;
using System.Globalization;
using System.Text;
using TrafficCourts.Domain.Models;
using TrafficCourts.OrdsDataService.Tco;
using X.PagedList;

namespace TrafficCourts.Staff.Service.Features.CourtFiles.Summaries;

public class Request : IRequest<Response>
{
    public bool? appearances { get; set; }
    public bool? notice_of_hearing_yn { get; set; }
    public bool? multiple_officers_yn { get; set; }
    public bool? electronic_ticket_yn { get; set; }

    public string? time_zone { get; set; }
    public string? submitted_from { get; set; }
    public string? submitted_thru { get; set; }

    public string? ticket_number { get; set; }
    public string? surname { get; set; }

    public string? jj_assigned_to { get; set; }
    public string? dispute_status_codes { get; set; }
    public string? appearance_courthouse_ids { get; set; }
    public string? to_be_heard_at_courthouse_ids { get; set; }

    public int? page_number { get; set; }
    public int? page_size { get; set; }

    public string? sort_by { get; set; }
}

public class Response
{
    public PagedDisputeCaseFileSummaryCollection Data { get; set; }
}

public class PagedDisputeCaseFileSummaryCollection
{
    public PagedDisputeCaseFileSummaryCollection()
    {
    }
    public PagedDisputeCaseFileSummaryCollection(IEnumerable<DisputeCaseFileSummary> items, int pageNumber, int pageSize, int totalRows)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRows = totalRows;
        TotalPages = (int)Math.Ceiling((double)totalRows / pageSize);
    }

    public IEnumerable<DisputeCaseFileSummary> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRows { get; set; }

}


public class Handler : IRequestHandler<Request, Response>
{
    private readonly IDisputeCaseFileSummaryRepository _repository;

    public Handler(IDisputeCaseFileSummaryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var parameters = GetParameters(request);

        var pagedCollection = await _repository.GetListAsync(parameters, cancellationToken);

        if (pagedCollection.Rows is not null)
        {
            var items = pagedCollection.Rows.Select(Map);

            var offset = pagedCollection.Offset;
            var pageSize = pagedCollection.Fetch;
            var totalRows = pagedCollection.TotalRows;

            int pageNumber = (offset / pageSize) + 1;
            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

            var pagedList = new PagedDisputeCaseFileSummaryCollection(items, pageNumber, pageSize, pagedCollection.TotalRows);
            return new Response { Data = pagedList };
        }

        return new Response(); // no data - todo
    }

    private Dictionary<string, string> GetParameters(Request request)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        AdjustRequest(request);

        AddInclude(parameters, request);
        AddWhere(parameters, request);
        AddOrderBy(parameters, request);
        AddPaging(parameters, request);

        return parameters;
    }

    private void AdjustRequest(Request request)
    {
        // ensure appearances are fetched if filtering or sorting on the column, otherwise we'd have to 
        // return bad request
        if (request.appearance_courthouse_ids is not null)
        {
            request.appearances = true;
        }

        if (request.sort_by is not null)
        {
            if (request.sort_by.Contains("appearanceCourthouseName") ||
                request.sort_by.Contains("appearanceRoomCode") ||
                request.sort_by.Contains("appearanceTs"))
            {
                request.appearances = true;
            }
        }

        // notice_of_hearing_yn
        // multiple_officers_yn
        // electronic_ticket_yn
    }

    private void AddInclude(Dictionary<string, string> parameters, Request request)
    {
        HashSet<string> include = new HashSet<string>();

        if (request.appearances is true) include.Add("appearances");
        if (request.notice_of_hearing_yn is true) include.Add("notice_of_hearing_yn");
        if (request.multiple_officers_yn is true) include.Add("multiple_officers_yn");
        if (request.electronic_ticket_yn is true) include.Add("electronic_ticket_yn");
        if (include.Count > 0) parameters.Add("include", string.Join(",", include));
    }

    private void AddWhere(Dictionary<string, string> parameters, Request request)
    {
        AddDateSubmittedFilter(parameters, request);

        if (request.ticket_number is not null) parameters.Add("ticket_number_txt_eq", request.ticket_number);
        if (request.surname is not null) parameters.Add("prof_surname_nm_eq", request.surname);
        if (request.jj_assigned_to is not null) parameters.Add("jj_assigned_to_eq", request.jj_assigned_to);

        if (request.dispute_status_codes is not null) parameters.Add("dispute_status_type_cd_in", request.dispute_status_codes);
        if (request.to_be_heard_at_courthouse_ids is not null) parameters.Add("to_be_heard_at_agen_id_in", request.to_be_heard_at_courthouse_ids);

        if (request.appearance_courthouse_ids is not null && request.appearances is true)
        {
            parameters.Add("appr_ctrm_agen_id_in", request.appearance_courthouse_ids);
        }

    }

    private void AddDateSubmittedFilter(Dictionary<string, string> parameters, Request request)
    {
        if (request.submitted_from is null && request.submitted_thru is null)
        {
            return;
        }

        var timeZone = request.time_zone ?? "America/Vancouver";
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

        // convert the from/thru to UTC
        if (request.submitted_from is not null)
        {
            DateTime date = DateTime.ParseExact(request.submitted_from, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            date = TimeZoneInfo.ConvertTimeToUtc(date, tz);
            parameters.Add("submitted_dt_ge", date.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        if (request.submitted_thru is not null)
        {
            // bump the date by one and search for less than the next day
            DateTime date = DateTime.ParseExact(request.submitted_thru, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            date = date.AddDays(1);
            date = TimeZoneInfo.ConvertTimeToUtc(date, tz);
            parameters.Add("submitted_dt_lt", date.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }

    private void AddOrderBy(Dictionary<string, string> parameters, Request request)
    {
        if (request.sort_by is null)
        {
            return;
        }

        StringBuilder buffer = new StringBuilder();
        string[] clientOrder = request.sort_by.Split(',');

        for (int i = 0; i < clientOrder.Length; i++)
        {
            string direction = "";
            var item = clientOrder[i];
            if (item.StartsWith('-'))
            {
                direction = "-";
                item = item.Substring(1);
            }

            // map the model to the column name
            string? target = item switch
            {
                "submittedTs" => "submitted_dt",
                "jjDecisionDate" => "jj_decision_dt",
                "signatoryName" => "signed_by",
                "hearingType" => "hearing_type_cd",
                "ticketNumber" => "ticket_number_txt",
                "violationDate" => "violation_dt",
                "toBeHeardAtCourthouseName" => "to_be_heard_at_agen_nm",
                "disputantSurname" => "prof_surname_nm",
                "disputantGivenName1" => "prof_given_1_nm",
                "disputeStatusDescription" => "dispute_status_type_dsc",
                "policeDetachment" => "detachment_agency_nm",
                "accidentYn" => "accident_yn",
                "noticeOfHearingYn" => "notice_of_hearing_yn",
                "multipleOfficersYn" => "multiple_officers_yn",
                "electronicTicketYn" => "electronic_ticket_yn",
                "jjAssignedTo" => "jj_assigned_to",
                "vtcAssignedTo" => "vtc_assigned_to",
                "vtcAssignedTs" => "vtc_assigned_dtm",
                "appearanceCourthouseName" => "appr_ctrm_agen_nm",
                "appearanceRoomCode" => "appr_ctrm_room_cd",
                "appearanceTs" => "appr_tm",
                _ => null
            };

            // did we try to sort on columns not included?
            if (target == "appr_ctrm_agen_nm" || target == "appr_ctrm_room_cd" || target == "appr_tm")
            {
                if (request.appearances is not true)
                {
                    continue;
                }
            }

            if (target == "notice_of_hearing_yn")
            {
                if (request.notice_of_hearing_yn is not true)
                {
                    continue;
                }
            }

            if (target == "multiple_officers_yn")
            {
                if (request.multiple_officers_yn is not true)
                {
                    continue;
                }
            }

            if (target == "electronic_ticket_yn")
            {
                if (request.electronic_ticket_yn is not true)
                {
                    continue;
                }
            }

            if (buffer.Length > 0)
            {
                buffer.Append(",");
            }
            buffer.Append(direction);
            buffer.Append(target);
        }


        parameters.Add("order", buffer.ToString());
    }

    private void AddPaging(Dictionary<string, string> parameters, Request request)
    {
        if (request.page_size is not null)
        {
            parameters.Add("fetch_rows", request.page_size.Value.ToString());
        }

        int pageSize = request.page_size ?? 25;

        if (request.page_number is not null)
        {
            // compute the offset_rows
            int offset = (request.page_number.Value - 1) * pageSize;
            parameters.Add("offset_rows", offset.ToString());
        }
    }


    private DisputeCaseFileSummary Map(OrdsDisputeCaseFileSummary dispute)
    {
        var summary = new DisputeCaseFileSummary
        {
            Id = dispute.dispute_id,
            SubmittedTs = dispute.submitted_dt,
            JjDecisionDate = dispute.jj_decision_dt,
            SignatoryName = dispute.signed_by,
            HearingType = ToJJDisputeHearingType(dispute.hearing_type_cd),
            TicketNumber = dispute.ticket_number_txt,
            ViolationDate = dispute.violation_dt,
            ViolationDateCount = dispute.unique_violation_dt_count,
            ToBeHeardAtCourthouseId = dispute.to_be_heard_at_agen_id,
            ToBeHeardAtCourthouseName = dispute.to_be_heard_at_agen_nm,
            DisputantSurname = dispute.prof_surname_nm,
            DisputantGivenName1 = dispute.prof_given_1_nm,
            DisputantGivenName2 = dispute.prof_given_2_nm,
            DisputantGivenName3 = dispute.prof_given_3_nm,
            Status = ToJJDisputeStatus(dispute.dispute_status_type_cd),
            DisputeStatusCd = dispute.dispute_status_type_cd,
            DisputeStatusDescription = dispute.dispute_status_type_dsc,
            PoliceDetachmentId = dispute.detachment_agen_id,
            PoliceDetachment = dispute.detachment_agency_nm,
            AccidentYn = ToYesNo(dispute.accident_yn),
            NoticeOfHearingYn = ToYesNo(dispute.notice_of_hearing_yn),
            MultipleOfficersYn = ToYesNo(dispute.multiple_officers_yn),
            ElectronicTicketYn = ToYesNo(dispute.electronic_ticket_yn),
            JjAssignedTo = dispute.jj_assigned_to,
            VtcAssignedTo = dispute.vtc_assigned_to,
            VtcAssignedTs = dispute.vtc_assigned_dtm,
            AppearanceCourthouseId = dispute.appr_ctrm_agen_id,
            AppearanceCourthouseName = dispute.appr_ctrm_agen_nm,
            AppearanceRoomCode = dispute.appr_ctrm_room_cd,
            AppearanceTs = dispute.appr_tm
        };

        return summary;
    }

    private static YesNo? ToYesNo(string? value)
    {
        return value switch
        {
            "Y" => YesNo.Yes,
            "N" => YesNo.No,
            null => null,
            _ => YesNo.Unknown
        };
    }

    private static JJDisputeStatus ToJJDisputeStatus(string value)
    {
        return value switch
        {
            "NEW" => JJDisputeStatus.NEW,
            "PROG" => JJDisputeStatus.IN_PROGRESS,
            "UPD" => JJDisputeStatus.DATA_UPDATE,
            "CONF" => JJDisputeStatus.CONFIRMED,
            "REQH" => JJDisputeStatus.REQUIRE_COURT_HEARING,
            "REQM" => JJDisputeStatus.REQUIRE_MORE_INFO,
            "ACCP" => JJDisputeStatus.ACCEPTED,
            "REV" => JJDisputeStatus.REVIEW,
            "HEAR" => JJDisputeStatus.HEARING_SCHEDULED,
            "CNLD" => JJDisputeStatus.CONCLUDED,
            "CANC" => JJDisputeStatus.CANCELLED,
            _ => JJDisputeStatus.UNKNOWN
        };
    }

    private static JJDisputeHearingType? ToJJDisputeHearingType(string? value)
    {
        return value switch
        {
            "C" => JJDisputeHearingType.COURT_APPEARANCE,
            "W" => JJDisputeHearingType.WRITTEN_REASONS,
            "U" => JJDisputeHearingType.UNKNOWN,
            null => null,
            _ => JJDisputeHearingType.UNKNOWN
        };
    }
}
