using System.Threading.Channels;
using TextFileSorter.Comparers;
using TextFileSorter.Factories;
using TextFileSorter.Interfaces.ProcessStages;
using TextFileSorter.Models;

namespace TextFileSorter.ProcessStages;

internal sealed class ChunkConsumer : IChunkConsumer
{
    private readonly string _chunkDirectoryPath;
    private readonly string _chunkFileExtension;
    private readonly TextLineComparer _textLineComparer = new();

    public ChunkConsumer(string chunkDirectoryPath, string chunkFileExtension)
    {
        _chunkDirectoryPath = chunkDirectoryPath;
        _chunkFileExtension = chunkFileExtension;
    }
    
    public async Task ConsumeAsync(Channel<Chunk> channel, CancellationToken cancellationToken)
    {
        await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProcessChunkAsync(chunk, cancellationToken);
        }
    }

    private async Task ProcessChunkAsync(Chunk chunk, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            Console.WriteLine($"Chunk {chunk.Index} - Sorting begin");
            chunk.TextLines.Sort(_textLineComparer);
            Console.WriteLine($"Chunk {chunk.Index} - Sorting end");
        }, cancellationToken);

        Console.WriteLine($"Chunk {chunk.Index} - Writing begin");

        var tempFilePath = Path.Combine(_chunkDirectoryPath, $"{chunk.Index:D4}{_chunkFileExtension}");
        await WriteChunkAsync(chunk, tempFilePath, cancellationToken);

        Console.WriteLine($"Chunk {chunk.Index} - Writing end");

        chunk.TextLines.Clear();
    }

    private async Task WriteChunkAsync(Chunk chunk, string filePath, CancellationToken cancellationToken)
    {
        using FileWriter fileWriter = FileProcessFactory.CreateFileWriter(filePath, chunk.SizeInBytes);

        foreach (var textLine in chunk.TextLines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await fileWriter.Writer.WriteLineAsync(textLine.Value);
        }

        await fileWriter.Writer.FlushAsync(cancellationToken);

        if (fileWriter.Stream.Length > fileWriter.Stream.Position)
        {
            fileWriter.Stream.SetLength(fileWriter.Stream.Position);
        }
    }
}
