using TextFileSorter.Models;

namespace TextFileSorter.Comparers;

internal sealed class TextLineComparer : IComparer<TextLine>
{
    public int Compare(TextLine leftElement, TextLine rightElement)
    {
        // get text part after dot using AsSpan to avoid unnecessary string allocations
        var leftElementStringPart = leftElement.Value.AsSpan(leftElement.DotIndex + 1);
        var rightElementStringPart = rightElement.Value.AsSpan(rightElement.DotIndex + 1);

        // compare text parts
        int compareResult = leftElementStringPart.CompareTo(rightElementStringPart, StringComparison.Ordinal);

        if (compareResult != 0)
        {
            return compareResult;
        }

        // compare number parts
        var leftElementNumberPart = leftElement.Value.AsSpan(0, leftElement.DotIndex);
        var rightElementNumberPart = rightElement.Value.AsSpan(0, rightElement.DotIndex);

        if (long.TryParse(leftElementNumberPart, out long leftNumber) &&
            long.TryParse(rightElementNumberPart, out long rightNumber))
        {
            return leftNumber.CompareTo(rightNumber);
        }

        return 0;
    }
}
