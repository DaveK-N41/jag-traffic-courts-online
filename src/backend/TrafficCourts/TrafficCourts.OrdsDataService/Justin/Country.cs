namespace TrafficCourts.OrdsDataService.Justin;

public record Country
{
#pragma warning disable IDE1006 // Naming Styles
    public int ctry_id { get; set; }
    public string ctry_short_nm { get; set; } = string.Empty;
    public string ctry_long_nm { get; set; } = string.Empty;
#pragma warning restore IDE1006 // Naming Styles
}
