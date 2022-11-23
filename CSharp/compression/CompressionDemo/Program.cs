// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using CompressionDemo;
using Perfolizer.Horology;

Console.WriteLine("Hello, World!");

//CompressionBenchmarks b = new CompressionBenchmarks();
//b.Lz4_Encode_Compress();


BenchmarkRunner.Run<CompressionBenchmarks>(
    ManualConfig.Create(DefaultConfig.Instance)
        .WithSummaryStyle(new BenchmarkDotNet.Reports.SummaryStyle(
            cultureInfo: System.Globalization.CultureInfo.CurrentCulture,
            printUnitsInHeader: true,
            timeUnit: TimeUnit.Millisecond, sizeUnit: SizeUnit.KB))
        );