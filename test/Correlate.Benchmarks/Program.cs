using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using Correlate.Benchmarks;

Runtime mostCurrentRuntime = CoreRuntime.Core80;
IToolchain mostCurrentToolchain = CsProjCoreToolchain.NetCoreApp80;

Version[] versions =
{
    new("4.0.0"), new("5.1.0"), new("0.0.0") // Current
};

IEnumerable<(Runtime, IToolchain)> runtimes = ConfigExtensions.GetRuntimes(args);
IEnumerable<(Version version, Runtime runtime, IToolchain toolchain)> runs =
    (
    from version in versions
    from runtime in runtimes
    orderby version descending, runtime.Item1.ToString() descending
    select (version, runtime.Item1, runtime.Item2)
    ).ToList();

#if RELEASE
IConfig cfg = DefaultConfig.Instance;
#else
IConfig cfg = new DebugInProcessConfig();
#endif
cfg = runs.Aggregate(cfg,
    (current, run) => current
        .ForAllRuntimes(
            run.version,
            run.runtime,
            run.toolchain,
            (runtime, toolchain) => runtime.Equals(mostCurrentRuntime) && toolchain.Equals(mostCurrentToolchain))
);

cfg.WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared));

BenchmarkRunner.Run<AspNetCoreBenchmark>(cfg);

internal static class ConfigExtensions
{
    public static IReadOnlyCollection<(Runtime, IToolchain)> GetRuntimes(string[] args)
    {
        const BindingFlags bf = BindingFlags.Public | BindingFlags.Static;

        var argRuntimes = args
            .SkipWhile(arg => arg == "--runtimes")
            .TakeWhile(arg => arg.StartsWith("net"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Runtime> runtimes = typeof(CoreRuntime).GetFields(bf)
            .Union(typeof(ClrRuntime).GetFields(bf))
            .Select(fi => fi.GetValue(null))
            .OfType<Runtime>()
            .Where(rt => argRuntimes.Contains(rt.RuntimeMoniker.ToString()))
            .ToList();

        IEnumerable<CsProjCoreToolchain> toolchains = typeof(CsProjCoreToolchain).GetFields(bf)
            .Select(fi => fi.GetValue(null))
            .OfType<CsProjCoreToolchain>()
            .ToList();

        return runtimes
            .Select(rt => (rt, (IToolchain)toolchains.Single(tc => ((CsProjGenerator)tc.Generator).TargetFrameworkMoniker == rt.MsBuildMoniker)))
            .ToList();
    }

    public static IConfig ForAllRuntimes(this IConfig config, Version version, Runtime runtime, IToolchain toolchain, Func<Runtime, IToolchain, bool> isBaselineRuntime)
    {
        bool isCurrentVersion = version.Major == 0;
        string label = isCurrentVersion
            ? "vNext"
            : $"v{version.ToString(3)}";

        Job job = JobMode<Job>.Default
            .WithRuntime(runtime)
            .WithToolchain(toolchain)
            .WithId(label);

        if (!isCurrentVersion)
        {
            job = job
                .WithNuGet("Correlate.AspNetCore", version.ToString(3))
                .WithNuGet("Correlate.DependencyInjection", version.ToString(3))
                .WithArguments(new[] { new MsBuildArgument("/p:CurrentVersion=false") });
        }
        else
        {
            if (isBaselineRuntime(runtime, toolchain))
            {
                job = job.AsBaseline();
            }
        }

        return config.AddJob(job);
    }
}
