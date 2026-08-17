using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using OptiPulse.Evaluation.Benchmarks;

// Constitution Principle II gate (tasks.md T018/T086): runs the evaluation
// benchmarks, then FAILS the process (non-zero exit) if any benchmark exceeds
// the 5ms budget or allocates on the hot path. CI treats a non-zero exit as a
// build failure, so a regression cannot merge.

const double MaxMeanMilliseconds = 5.0;
const long MaxAllocatedBytes = 0;

var config = DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator);
var summaries = BenchmarkRunner.Run(typeof(EvaluationBenchmarks).Assembly, config, args);

int violations = 0;

foreach (var summary in summaries)
{
    foreach (var report in summary.Reports)
    {
        var name = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;

        if (!report.Success)
        {
            Console.Error.WriteLine($"GATE FAIL [{name}]: benchmark did not complete successfully.");
            violations++;
            continue;
        }

        var meanNs = report.ResultStatistics?.Mean ?? double.NaN;
        var meanMs = meanNs / 1_000_000.0;
        if (double.IsNaN(meanNs) || meanMs > MaxMeanMilliseconds)
        {
            Console.Error.WriteLine(
                $"GATE FAIL [{name}]: mean {meanMs:F6} ms exceeds the {MaxMeanMilliseconds} ms budget (Principle II).");
            violations++;
        }

        var allocated = report.GetResultRuns().Count == 0
            ? (long?)null
            : report.Metrics.TryGetValue("Allocated Memory", out var metric) ? (long)metric.Value : null;

        if (allocated is > MaxAllocatedBytes)
        {
            Console.Error.WriteLine(
                $"GATE FAIL [{name}]: allocated {allocated} B/op, expected {MaxAllocatedBytes} B (Principle II zero-allocation).");
            violations++;
        }
    }
}

if (violations > 0)
{
    Console.Error.WriteLine($"\n{violations} benchmark gate violation(s) — see above (Constitution Principle II).");
    return 1;
}

Console.WriteLine("\nBenchmark gate PASSED: all evaluations within 5 ms and zero-allocation.");
return 0;
