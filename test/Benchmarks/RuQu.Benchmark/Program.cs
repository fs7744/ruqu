using BenchmarkDotNet.Running;
using RuQu.Benchmark;

var a = new CsvTest();
//a.RuQu_Read_Csv_file_string();
//a.CsvHelper_file();
//a.Sylvan_Read_Csv();
//a.Sep_Read_Csv();
//a.CsvHelper_Read();
//a.RuQu_Read_Csv_String();
//a.RuQu_Read_Csv_StringReader();
//var a = new IniTest();
//a.RuQu_Chunk_Read_Ini();
var summary = BenchmarkRunner.Run(typeof(Program).Assembly);