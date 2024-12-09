namespace TrafficCourts.Staff.Service.Features.CourtFiles.Summaries;

public class Response
{
    public Response(PagedDisputeCaseFileSummaryCollection data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public Response(string errorId)
    {
        ErrorId = errorId ?? throw new ArgumentNullException(nameof(errorId));
    }

    public PagedDisputeCaseFileSummaryCollection? Data { get; set; }

    public string? ErrorId { get; set; }
}
