using BenchmarkDotNet.Running;
using RuQu.Benchmark;

var a = new CsvTest();
a.CsvHelper_Read();
a.RuQu_Read_Csv_String();
a.RuQu_Read_Csv_StringReader();
var summary = BenchmarkRunner.Run(typeof(Program).Assembly);