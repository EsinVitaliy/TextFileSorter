namespace TextFileSorter.Models;

internal readonly struct FileWriter : IDisposable
{
    public FileStream Stream { get; init; }

    public StreamWriter Writer { get; init; }

    public void Dispose()
    {
        Writer?.Dispose();
        Stream?.Dispose();
    }
}
