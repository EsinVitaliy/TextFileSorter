namespace TextFileSorter.Models;

internal readonly struct TextLine
{
    public readonly string Value;
    public readonly int DotIndex;

    public TextLine(string line)
    {
        Value = line;
        DotIndex = line.IndexOf('.');

        if (DotIndex == -1)
        {
            throw new Exception("Line does not contain a dot");
        }
    }
}
