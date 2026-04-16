using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextFileSorter.DecisionBenchmarks.Comparers;
using TextFileSorter.DecisionBenchmarks.Models;

namespace TextFileSorter.DecisionBenchmarks;

// TextLineV2 sorting

//| Method                        | Mean     | Error    | StdDev   | Gen0      | Gen1      | Gen2      | Allocated    |
//|------------------------------ |---------:|---------:|---------:|----------:|----------:|----------:|-------------:|
//| List_TextLineV2_sort          | 10.810 s | 0.1861 s | 0.1741 s |         - |         - |         - |         88 B |
//| List_TextLineV2_parallel_sort |  7.453 s | 0.0702 s | 0.0623 s | 1000.0000 | 1000.0000 | 1000.0000 | 7512465240 B |

[MemoryDiagnoser]
public class TextLineV2SortBenchmarks
{
    private readonly List<TextLineV2> _list = [];
    private readonly int _newLineSizeInBytes = Encoding.UTF8.GetByteCount(Environment.NewLine);
    private readonly TextLineComparerV2 _comparer = new();

    [GlobalSetup]
    public void Setup()
    {
        int sizeInMegabytes = 0;
        int maxSizeInBytes = 1024 * 1024 * 1024;

        while (sizeInMegabytes < maxSizeInBytes)
        {
            string textLine = Generator.Program.GenerateTextLine();
            sizeInMegabytes += Encoding.UTF8.GetByteCount(textLine) + _newLineSizeInBytes;
            _list.Add(new TextLineV2(textLine));
        }
    }

    [Benchmark]
    public void List_TextLineV2_sort()
    {
        _list.Sort(_comparer);
    }

    [Benchmark]
    public void List_TextLineV2_parallel_sort()
    {
        _list
            .AsParallel()
            .OrderBy(s => s, _comparer);
    }
}
