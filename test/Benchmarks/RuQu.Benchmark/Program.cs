using BenchmarkDotNet.Running;
using RuQu.Benchmark;

//var a = new CsvTest();
//a.CsvHelper_Read();
//a.RuQu_Read_Csv_String();
//a.RuQu_Read_Csv_StringReader();
var a = new IniTest();
a.RuQu_Chunk_Read_Ini();
var summary = BenchmarkRunner.Run(typeof(Program).Assembly);