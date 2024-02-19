using BenchmarkDotNet.Running;
using RuQu.Benchmark;
new HexColorTest().RuQu_HexColorStruct();
var summary = BenchmarkRunner.Run(typeof(Program).Assembly);