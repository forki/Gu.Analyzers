``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 2 (10.0.15063)
Processor=Intel Core i7-3667U CPU 2.00GHz (Ivy Bridge), ProcessorCount=4
Frequency=2435876 Hz, Resolution=410.5299 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2115.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2115.0


```
 |                        Method |     Mean |     Error |   StdDev |   Median |   Gen 0 | Allocated |
 |------------------------------ |---------:|----------:|---------:|---------:|--------:|----------:|
 | RunOnGuAnalyzersProject | 9.340 ms | 0.3903 ms | 1.101 ms | 8.896 ms | 62.5000 | 139.38 KB |