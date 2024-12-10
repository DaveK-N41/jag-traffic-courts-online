using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using TrafficCourts.Diagnostics;

namespace TrafficCourts.OrdsDataService;

internal class OrdsDataServiceOperationMetrics : OperationMetrics, IOrdsDataServiceOperationMetrics
{
    public const string MeterName = "OrdsDataService";

    public OrdsDataServiceOperationMetrics(IMeterFactory factory) : base(factory, MeterName, "ordsdataservice", "ORDS Data Service")
    {
    }
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
