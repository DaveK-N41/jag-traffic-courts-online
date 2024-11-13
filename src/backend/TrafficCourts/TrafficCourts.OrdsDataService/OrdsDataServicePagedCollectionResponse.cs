using System.Text.Json.Serialization;

namespace TrafficCourts.OrdsDataService;

public partial record OrdsDataServicePagedCollectionResponse<T> : OrdsDataServiceCollectionResponse<T>
{
    /// <summary>
    /// The total number of rows matching the query.
    /// </summary>
    [JsonPropertyName("total")]
    public int TotalRows { get; set; }

    /// <summary>
    /// The zero-based index of the first row to return.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    /// <summary>
    /// The number of rows to return.
    /// </summary>
    [JsonPropertyName("fetch")]
    public int Fetch { get; set; }
}
