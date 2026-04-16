using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextFileSorter.Comparers;
using TextFileSorter.Models;

namespace TextFileSorter.DecisionBenchmarks;

// TextLine sorting

//| Method             | Mean    | Error    | StdDev   | Gen0      | Gen1      | Gen2      | Allocated    |
//|------------------- |--------:|---------:|---------:|----------:|----------:|----------:|-------------:|
//| List_standard_sort | 6.841 s | 0.1306 s | 0.1698 s |         - |         - |         - |         88 B |
//| List_parallel_sort | 5.859 s | 0.0777 s | 0.0689 s | 1000.0000 | 1000.0000 | 1000.0000 | 5077051288 B |

[MemoryDiagnoser]
public class TextLineSortBenchmarks
{
    private readonly List<TextLine> _list = [];
    private readonly int _newLineSizeInBytes = Encoding.UTF8.GetByteCount(Environment.NewLine);
    private readonly TextLineComparer _comparer = new();

    [GlobalSetup]
    public void Setup()
    {
        int sizeInMegabytes = 0;
        int maxSizeInBytes = 1024 * 1024 * 1024;

        while (sizeInMegabytes < maxSizeInBytes)
        {
            string textLine = Generator.Program.GenerateTextLine();
            sizeInMegabytes += Encoding.UTF8.GetByteCount(textLine) + _newLineSizeInBytes;
            _list.Add(new TextLine(textLine));
        }
    }

    [Benchmark]
    public void List_standard_sort()
    {
        _list.Sort(_comparer);
    }
    
    [Benchmark]
    public void List_parallel_sort()
    {
        _list
            .AsParallel()
            .OrderBy(s => s, _comparer);
    }
}
