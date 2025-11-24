using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Correlate.Benchmarks;

public static class Program
{
    private const string RunAllSwitch = "--all";

    public static void Main(string[] args)
    {
        IConfig config = ManualConfig.CreateMinimumViable();
#if DEBUG
        config = ManualConfig.Union(config, new DebugInProcessConfig());
#endif

        Assembly asm = typeof(Program).Assembly;
        if (args.Contains(RunAllSwitch))
        {
            args = args.Except([RunAllSwitch]).ToArray();
            BenchmarkRunner.Run(asm, config, args);
        }
        else
        {
            BenchmarkSwitcher.FromAssembly(asm).Run(args, config);
        }
    }
}
