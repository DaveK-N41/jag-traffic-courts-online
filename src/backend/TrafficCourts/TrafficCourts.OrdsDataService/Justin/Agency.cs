
namespace TrafficCourts.OrdsDataService.Justin;

public record Agency
{
#pragma warning disable IDE1006 // Naming Styles
    public decimal agen_id { get; set; }
    public string agen_agency_nm { get; set; } = string.Empty;
    public string cdat_agency_type_cd { get; set; } = string.Empty;
    public string agen_active_yn { get; set; } = string.Empty;
#pragma warning restore IDE1006 // Naming Styles
}
