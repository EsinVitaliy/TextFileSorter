using System;
using System.Collections.Generic;
using TextFileSorter.DecisionBenchmarks.Models;

namespace TextFileSorter.DecisionBenchmarks.Comparers;

internal class TextLineComparerV2 : IComparer<TextLineV2>
{
    public int Compare(TextLineV2 leftElement, TextLineV2 rightElement)
    {
        var leftElementSpan = leftElement.Value.Span;
        var rightElementSpan = rightElement.Value.Span;

        var leftElementStringPart = leftElementSpan.Slice(leftElement.DotIndex + 1);
        var rightElementStringPart = rightElementSpan.Slice(rightElement.DotIndex + 1);

        // compare text parts
        int compareResult = leftElementStringPart.SequenceCompareTo(rightElementStringPart);

        if (compareResult != 0)
        {
            return compareResult;
        }

        // compare number parts
        var leftElementNumberPart = leftElementSpan.Slice(0, leftElement.DotIndex);
        var rightElementNumberPart = rightElementSpan.Slice(0, rightElement.DotIndex);

        if (long.TryParse(leftElementNumberPart, out long leftNumber) &&
            long.TryParse(rightElementNumberPart, out long rightNumber))
        {
            return leftNumber.CompareTo(rightNumber);
        }

        return 0;
    }
}