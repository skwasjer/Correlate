# Benchmark results

```
BenchmarkDotNet v0.13.7, Windows 10 (10.0.19045.3324/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 7.0.400
  [Host]  : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2
  4.0.0   : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2
  Current : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2
```

|  Method |     Job |         Arguments |                                                NuGetReferences | Toolchain |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|-------- |-------- |------------------ |--------------------------------------------------------------- |---------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
| ApiCall |   4.0.0 | /p:Baseline=false | Correlate.AspNetCore 4.0.0,Correlate.DependencyInjection 4.0.0 |  .NET 6.0 | 148.2 us | 1.95 us | 1.73 us |  1.02 |    0.02 | 0.4883 |   4.33 KB |        1.11 |
| ApiCall |   4.0.0 | /p:Baseline=false | Correlate.AspNetCore 4.0.0,Correlate.DependencyInjection 4.0.0 |  .NET 7.0 | 148.1 us | 0.93 us | 0.87 us |  1.02 |    0.01 | 0.4883 |   4.06 KB |        1.04 |
| ApiCall | Current |           Default |                                                        Default |  .NET 6.0 | 140.2 us | 1.12 us | 1.05 us |  0.96 |    0.01 | 0.4883 |   4.16 KB |        1.07 |
| ApiCall | Current |           Default |                                                        Default |  .NET 7.0 | 145.8 us | 1.07 us | 1.00 us |  1.00 |    0.00 | 0.4883 |   3.91 KB |        1.00 |

### CLI

To run the benchmark:
```
cd ./test/Correlate.Benchmarks
dotnet run -c Release -f net7.0 net6.0
```
