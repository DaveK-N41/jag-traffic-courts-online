using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using TrafficCourts.Diagnostics;

namespace TrafficCourts.OrdsDataService;

internal class OrdsDataServiceOperationMetrics : OperationMetrics, IOrdsDataServiceOperationMetrics
{
    public const string MeterName = "OrdsDataService";

    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;

    public OrdsDataServiceOperationMetrics(IMeterFactory factory) : base(factory, MeterName, "ordsdataservice", "ORDS Data Service")
    {
        _cacheHitCounter = Meter.CreateCounter<long>("ordsdataservice.etag.cache.hits");
        _cacheMissCounter = Meter.CreateCounter<long>("ordsdataservice.etag.cache.misses");
    }

    /// <summary>
    /// Records a OrdsDataService ETag cache hit.
    /// </summary>
    public void RecordEtagCacheHit(KeyValuePair<string, object?> tag1, KeyValuePair<string, object?> tag2) => _cacheHitCounter.Add(1, tag1, tag2);
    /// <summary>
    /// Records a OrdsDataService ETag cache miss.
    /// </summary>
    public void RecordEtagCacheMiss(KeyValuePair<string, object?> tag1, KeyValuePair<string, object?> tag2) => _cacheMissCounter.Add(1, tag1, tag2);
}

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddOrdsDataServiceInstrumentation(this TracerProviderBuilder builder)
    {
        //builder.AddSource("...");
        return builder;
    }
}

/// <summary>
/// Contains extension methods to <see cref="MeterProviderBuilder"/> for enabling ORDS Data Service metrics instrumentation.
/// </summary>
public static class MeterProviderBuilderExtensions
{
    /// <summary>
    /// Adds meters related to ORDS Data Service.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MeterProviderBuilder AddOrdsDataServiceInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(OrdsDataServiceOperationMetrics.MeterName);
        return builder;
    }
}
