#nullable enable
using System.Text;
using System.Threading.Channels;
using TextFileSorter.Factories;
using TextFileSorter.Interfaces.ProcessStages;
using TextFileSorter.Models;

namespace TextFileSorter.ProcessStages;

internal sealed class ChunkProducer : IChunkProducer
{
    private readonly string _filePath;
    private readonly long _maxChunkSizeInBytes;

    private readonly Encoding _chunkEncoding = Encoding.UTF8;
    private readonly int _newLineLength = Encoding.UTF8.GetByteCount(Environment.NewLine);

    private string? _overflowTextLine;

    public ChunkProducer(string filePath, long maxChunkSizeInBytes)
    {
        _filePath = filePath;
        _maxChunkSizeInBytes = maxChunkSizeInBytes;
    }

    public async Task ProduceAsync(ChannelWriter<Chunk> channelWriter, CancellationToken cancellationToken)
    {
        using FileReader fileReader = FileProcessFactory.CreateFileReader(_filePath);

        int chunkIndex = 0;

        try
        {
            Chunk chunk;

            while ((chunk = await CreateChunkAsync(fileReader.Reader, chunkIndex, cancellationToken)).TextLines.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await channelWriter.WriteAsync(chunk, cancellationToken);
                chunkIndex++;
            }

            channelWriter.Complete();
        }
        catch (Exception ex)
        {
            channelWriter.Complete(ex);
            throw;
        }
    }
    
    private async Task<Chunk> CreateChunkAsync(StreamReader reader, int chunkIndex, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Chunk {chunkIndex} - Creation begin");

        List<TextLine> textLines = [];
        int currentChunkSizeInBytes = 0;

        if (_overflowTextLine is not null)
        {
            textLines.Add(new TextLine(_overflowTextLine));
            currentChunkSizeInBytes = _chunkEncoding.GetByteCount(_overflowTextLine) + _newLineLength;

            _overflowTextLine = null;
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? textLine = await reader.ReadLineAsync(cancellationToken);

            if (textLine is null)
            {
                break;
            }

            int textLineSizeInBytes = _chunkEncoding.GetByteCount(textLine) + _newLineLength;

            if (currentChunkSizeInBytes + textLineSizeInBytes > _maxChunkSizeInBytes)
            {
                if (textLineSizeInBytes > _maxChunkSizeInBytes)
                {
                    throw new Exception("Text line size is bigger than chunk size");
                }

                _overflowTextLine = textLine;
                break;
            }

            currentChunkSizeInBytes += textLineSizeInBytes;
            textLines.Add(new TextLine(textLine));
        }

        string chunkStatus = textLines.Count == 0
            ? "Chunk is empty, abort creation"
            : "Creation end";

        Console.WriteLine($"Chunk {chunkIndex} - {chunkStatus}");

        return new()
        {
            Index = chunkIndex,
            TextLines = textLines,
            SizeInBytes = currentChunkSizeInBytes
        };
    }
}
