namespace TrafficCourts.OrdsDataService.Justin;

public record City
{
#pragma warning disable IDE1006 // Naming Styles
    public int ctry_id { get; set; }
    public int city_seq_no { get; set; }
    public string city_long_nm { get; set; } = string.Empty;
    public int? prov_ctry_id { get; set; }
    public int? prov_seq_no { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}
