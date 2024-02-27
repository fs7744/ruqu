using BenchmarkDotNet.Running;
//using RuQu.Benchmark;

//var a = new CsvTest();
//a.CsvHelper_Read();
//a.RuQu_Read_Csv();
var summary = BenchmarkRunner.Run(typeof(Program).Assembly);