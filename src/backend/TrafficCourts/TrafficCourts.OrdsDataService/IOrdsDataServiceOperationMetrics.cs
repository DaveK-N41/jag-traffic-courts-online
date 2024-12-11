using TrafficCourts.Diagnostics;

namespace TrafficCourts.OrdsDataService;

internal interface IOrdsDataServiceOperationMetrics : IOperationMetrics
{
    /// <summary>
    /// Records a OrdsDataService ETag cache hit.
    /// </summary>
    void RecordEtagCacheHit(KeyValuePair<string, object?> tag1, KeyValuePair<string, object?> tag2);

    /// <summary>
    /// Records a OrdsDataService ETag cache miss.
    /// </summary>
    void RecordEtagCacheMiss(KeyValuePair<string, object?> tag1, KeyValuePair<string, object?> tag2);
}
