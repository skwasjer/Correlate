# Benchmark results

```
BenchmarkDotNet v0.15.6, Windows 10 (10.0.19045.6456/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.100
  [Host] : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  vNext  : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
```

| Method  | Runtime   | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------- |---------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ApiCall | .NET 10.0 | 83.23 us | 1.631 us | 1.526 us |  0.90 |    0.03 | 0.4883 |   3.95 KB |        1.00 |
| ApiCall | .NET 8.0  | 92.32 us | 1.815 us | 2.229 us |  1.00 |    0.03 | 0.4883 |   3.96 KB |        1.00 |
| ApiCall | .NET 9.0  | 85.85 us | 1.371 us | 1.215 us |  0.93 |    0.03 | 0.4883 |   3.98 KB |        1.01 |


### CLI

To run the benchmark:
```
cd ./test/Correlate.Benchmarks
dotnet run -c Release -f net10.0 --runtimes net10_0 net90 net80
```
