namespace TrafficCourts.OrdsDataService.Justin;

public record Language
{
#pragma warning disable IDE1006 // Naming Styles
    public string cdln_language_cd { get; set; } = string.Empty;
    public string cdln_language_dsc { get; set; } = string.Empty;
    public string cdln_active_yn { get; set; } = string.Empty;
#pragma warning restore IDE1006 // Naming Styles
}
