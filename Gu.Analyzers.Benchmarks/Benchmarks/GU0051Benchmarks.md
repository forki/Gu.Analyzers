``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                  Method |     Mean |    Error |   StdDev |  Gen 0 |  Gen 1 | Allocated |
 |------------------------ |---------:|---------:|---------:|-------:|-------:|----------:|
 | RunOnGuAnalyzersProject | 276.4 us | 5.497 us | 10.46 us | 2.9297 | 0.4883 |  21.04 KB |
