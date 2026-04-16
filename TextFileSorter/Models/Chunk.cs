namespace TextFileSorter.Models;

internal readonly struct Chunk
{
    public int Index { get; init; }

    public List<TextLine> TextLines { get; init; }

    public int SizeInBytes { get; init; }
}
