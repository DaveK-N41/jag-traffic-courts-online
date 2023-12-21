﻿using System.Diagnostics;
using System.Diagnostics.Metrics;
using TrafficCourts.Diagnostics;
using TrafficCourts.Common.OpenAPIs.OracleDataApi.v1_0;
using Timer = TrafficCourts.Diagnostics.Timer;

namespace TrafficCourts.Workflow.Service;

public static class Instrumentation
{
    public const string MeterName = "WorkflowService";

    private static readonly Meter _meter;

    private static readonly Timer _smtpOperation;
    private static readonly Counter<long> _smtpOperationErrorTotal;

    static Instrumentation()
    {
        _meter = new Meter(MeterName);

        // SMTP (email) operations
        _smtpOperation = new Timer(_meter, "smtp.operation.duration", "ms", "Elapsed time spent executing a smtp operation");
        _smtpOperationErrorTotal = _meter.CreateCounter<long>("smtp.operation.errors", "ea", "Number of times a smtp operation not be completed due to an error");
    }

    private static ITimerOperation BeginOperation(Timer timer, string operation)
    {
        Debug.Assert(timer != null);
        Debug.Assert(operation != null);

        if (operation.EndsWith("Async"))
        {
            operation = operation[..^5];
        }

        return timer.Start(new TagList { { "operation", operation } });
    }

    public static class Smtp
    {
        public static ITimerOperation BeginOperation(string operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            return Instrumentation.BeginOperation(_smtpOperation, operation);
        }

        /// <summary>
        /// Indicates an operation ended with an error.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="exception"></param>
        public static void EndOperation(ITimerOperation operation, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(exception);

            // let the timer know there was an excetion
            operation.Error(exception);

            // increment the error counter and record the same tags as the operation
            _smtpOperationErrorTotal.Add(1, operation.Tags);
        }
    }
}
