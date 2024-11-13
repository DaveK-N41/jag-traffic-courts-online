using System.Text.Json.Serialization;

namespace TrafficCourts.OrdsDataService;

public partial record OrdsDataServiceCollectionResponse<T>
{
    [JsonPropertyName("rows")]
    public List<T>? Rows { get; set; }

    [JsonPropertyName("errors")]
    public List<OrdsDataServiceError>? Errors { get; set; }
}
