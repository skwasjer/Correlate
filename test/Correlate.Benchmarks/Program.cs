using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using Correlate.Benchmarks;

IToolchain[] toolchains = { CsProjCoreToolchain.NetCoreApp70, CsProjCoreToolchain.NetCoreApp60 };

IConfig cfg = DefaultConfig.Instance
    .ForAllToolchains(toolchains, Job.Default.WithId("Current"), toolchain => toolchain.Equals(CsProjCoreToolchain.NetCoreApp70))
    .ForAllToolchains(toolchains,
        Job.Default
            .WithId("4.0.0")
            .WithToolchain(CsProjCoreToolchain.NetCoreApp70)
            .WithNuGet("Correlate.AspNetCore", "4.0.0")
            .WithNuGet("Correlate.DependencyInjection", "4.0.0")
            .WithArguments(new[] { new MsBuildArgument("/p:Baseline=false") })
    );

#if RELEASE
BenchmarkRunner.Run<AspNetCoreBenchmark>(cfg);
#else
BenchmarkRunner.Run<AspNetCoreBenchmark>(new DebugInProcessConfig());
#endif

internal static class ConfigExtensions
{
    public static IConfig ForAllToolchains(this IConfig config, IEnumerable<IToolchain> toolchains, Job job, Func<IToolchain, bool>? isBaseline = null)
    {
        foreach (IToolchain toolchain in toolchains)
        {
            Job j = job.WithToolchain(toolchain);
            config = config.AddJob(isBaseline?.Invoke(toolchain) ?? false ? j.AsBaseline() : j);
        }

        return config;
    }
}
