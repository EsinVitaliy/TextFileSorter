using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TextFileSorter.Benchmarks;

[SimpleJob(RunStrategy.Monitoring, launchCount: 1, warmupCount: 0, iterationCount: 1)]
public class SortBenchmarks
{
    private string _generatedFilePath;
    private string _sortedFilePath;

    [GlobalSetup]
    public async Task Setup()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        _generatedFilePath = Path.Join(basePath, "generated.txt");
        _sortedFilePath = _generatedFilePath + "_sorted.txt";

        int generatedFileSizeInMegabytes = 100;
        await Generator.Program.GenerateFile(_generatedFilePath, generatedFileSizeInMegabytes);
    }

    [Benchmark]
    public async Task Sort_100Mb_file()
    {
        await TextFileSorter.Program.RunAsync(_generatedFilePath, _sortedFilePath, 0, CancellationToken.None);
    }
}
