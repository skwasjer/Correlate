# Benchmark results

```
BenchmarkDotNet v0.15.2, Windows 10 (10.0.19045.6216/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.100-preview.7.25380.108
  [Host] : .NET 10.0.0 (10.0.25.38108), X64 RyuJIT AVX2
  vNext  : .NET 10.0.0 (10.0.25.38108), X64 RyuJIT AVX2
```

| Method  | Runtime   | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------- |---------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ApiCall | .NET 10.0 | 83.28 us | 1.210 us | 1.441 us |  0.95 |    0.02 | 0.4883 |   3.77 KB |        1.00 |
| ApiCall | .NET 8.0  | 87.56 us | 1.231 us | 1.028 us |  1.00 |    0.02 | 0.4883 |   3.77 KB |        1.00 |
| ApiCall | .NET 9.0  | 81.35 us | 0.522 us | 0.580 us |  0.93 |    0.01 | 0.4883 |    3.8 KB |        1.01 |


### CLI

To run the benchmark:
```
cd ./test/Correlate.Benchmarks
dotnet run -c Release -f net10.0 /p:NetPreview=true --runtimes net10_0 net90 net80
```
