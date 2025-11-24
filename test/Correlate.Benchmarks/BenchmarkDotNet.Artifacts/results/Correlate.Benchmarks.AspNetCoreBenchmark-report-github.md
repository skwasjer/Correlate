```

BenchmarkDotNet v0.15.7, Windows 10 (10.0.19045.6575/22H2/2022Update)
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.100
  [Host]    : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  .NET 10.0 : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  .NET 8.0  : .NET 8.0.22 (8.0.22, 8.0.2225.52707), X64 RyuJIT x86-64-v3


```
| Method  | Job       | Runtime   | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------- |---------- |---------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| ApiCall | .NET 10.0 | .NET 10.0 | 84.45 μs | 1.608 μs | 1.579 μs |  1.00 |    0.03 | 0.4883 |   3.95 KB |        1.00 |
| ApiCall | .NET 8.0  | .NET 8.0  | 89.54 μs | 1.733 μs | 2.129 μs |  1.06 |    0.03 | 0.4883 |   3.96 KB |        1.00 |
