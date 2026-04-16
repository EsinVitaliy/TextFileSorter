using System;

namespace TextFileSorter.DecisionBenchmarks.Models;

internal readonly struct TextLineV2
{
    public readonly ReadOnlyMemory<char> Value;
    public readonly int DotIndex;

    public TextLineV2(string line)
    {
        Value = line.AsMemory();
        DotIndex = line.IndexOf('.');

        if (DotIndex == -1)
        {
            throw new Exception("Line does not contain a dot");
        }
    }
}
