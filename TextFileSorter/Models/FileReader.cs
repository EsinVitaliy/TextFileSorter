namespace TextFileSorter.Models;

internal readonly struct FileReader : IDisposable
{
    public FileStream Stream { get; init; }

    public StreamReader Reader { get; init; }

    public void Dispose()
    {
        Reader?.Dispose();
        Stream?.Dispose();
    }
}
