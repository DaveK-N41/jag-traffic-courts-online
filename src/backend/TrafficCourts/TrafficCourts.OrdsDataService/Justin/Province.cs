namespace TrafficCourts.OrdsDataService.Justin;

public record Province
{
#pragma warning disable IDE1006 // Naming Styles
    public int ctry_id { get; set; }
    public int prov_seq_no { get; set; }
    public string prov_nm { get; set; } = string.Empty;
    public string prov_abbreviation_cd { get; set; } = string.Empty;
#pragma warning restore IDE1006 // Naming Styles
}
