# Benchmark results

```
BenchmarkDotNet v0.13.10, Windows 10 (10.0.19045.3693/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.100
  [Host] : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  v4.0.0 : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  v5.1.0 : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
  vNext  : .NET 6.0.25 (6.0.2523.51912), X64 RyuJIT AVX2
```

| Method  | Job    | Runtime  | Arguments               | NuGetReferences                                                | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------- |------- |--------- |------------------------ |--------------------------------------------------------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ApiCall | v4.0.0 | .NET 6.0 | /p:CurrentVersion=false | Correlate.AspNetCore 4.0.0,Correlate.DependencyInjection 4.0.0 | 169.6 us | 1.53 us | 1.36 us |  1.04 |    0.01 | 0.4883 |   4.33 KB |        1.15 |
| ApiCall | v4.0.0 | .NET 8.0 | /p:CurrentVersion=false | Correlate.AspNetCore 4.0.0,Correlate.DependencyInjection 4.0.0 | 165.5 us | 2.35 us | 2.20 us |  1.02 |    0.02 | 0.4883 |   3.87 KB |        1.03 |
| ApiCall | v5.1.0 | .NET 6.0 | /p:CurrentVersion=false | Correlate.AspNetCore 5.1.0,Correlate.DependencyInjection 5.1.0 | 168.7 us | 2.30 us | 2.15 us |  1.03 |    0.01 | 0.4883 |   4.17 KB |        1.11 |
| ApiCall | v5.1.0 | .NET 8.0 | /p:CurrentVersion=false | Correlate.AspNetCore 5.1.0,Correlate.DependencyInjection 5.1.0 | 162.6 us | 2.31 us | 2.05 us |  0.99 |    0.01 | 0.4883 |   3.75 KB |        1.00 |
| ApiCall | vNext  | .NET 6.0 | Default                 | Default                                                        | 166.2 us | 1.70 us | 1.50 us |  1.02 |    0.02 | 0.4883 |   4.17 KB |        1.11 |
| ApiCall | vNext  | .NET 8.0 | Default                 | Default                                                        | 163.2 us | 1.60 us | 1.34 us |  1.00 |    0.00 | 0.4883 |   3.75 KB |        1.00 |

### CLI

To run the benchmark:
```
cd ./test/Correlate.Benchmarks
dotnet run -c Release -f net9.0 --runtimes net90 net80
```
