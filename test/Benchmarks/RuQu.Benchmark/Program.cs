using BenchmarkDotNet.Running;
using RuQu.Benchmark;

BenchmarkRunner.Run<IniTest>();
//var summary = BenchmarkRunner.Run(typeof(Program).Assembly);