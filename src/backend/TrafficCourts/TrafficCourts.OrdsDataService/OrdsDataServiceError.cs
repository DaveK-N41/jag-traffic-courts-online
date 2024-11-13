using System.Text.Json.Serialization;

namespace TrafficCourts.OrdsDataService;

public partial record OrdsDataServiceError
{
    [JsonPropertyName("error_code")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("error_msg")]
    public string ErrorMessage { get; set; } = string.Empty;

    [JsonPropertyName("error_stack")]
    public string ErrorStack { get; set; } = string.Empty;
}
