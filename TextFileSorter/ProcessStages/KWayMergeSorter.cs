using TextFileSorter.Comparers;
using TextFileSorter.Factories;
using TextFileSorter.Interfaces.ProcessStages;
using TextFileSorter.Models;

namespace TextFileSorter.ProcessStages;

internal class KWayMergeSorter : IChunkMergeSorter
{
    private readonly string _chunkDirectoryPath;
    private readonly string _chunkSearchPattern;
    private readonly string _outputFilePath;
    private readonly long _approximateOutputFileSizeInBytes;

    public KWayMergeSorter(string chunkDirectoryPath, string chunkFileExtension, string outputFilePath, long approximateOutputFileSizeInBytes)
    {
        _chunkDirectoryPath = chunkDirectoryPath;
        _chunkSearchPattern = "*" + chunkFileExtension;
        _outputFilePath = outputFilePath;
        _approximateOutputFileSizeInBytes = approximateOutputFileSizeInBytes;
    }

    public async Task MergeAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Merge sort begin");

        string[] chunkPaths = Directory.GetFiles(_chunkDirectoryPath, _chunkSearchPattern);
        PriorityQueue<FileReader, TextLine> priorityQueue = new(new TextLineComparer());

        foreach (string chunk in chunkPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            FileReader chunkReader = FileProcessFactory.CreateFileReader(chunk);
            string minValue;

            if ((minValue = await chunkReader.Reader.ReadLineAsync(cancellationToken)) is not null)
            {
                priorityQueue.Enqueue(chunkReader, new TextLine(minValue));
            }
            else
            {
                // chunk is empty or invalid
                chunkReader.Dispose();
            }
        }

        using var sortedFileWriter = FileProcessFactory.CreateFileWriter(_outputFilePath, _approximateOutputFileSizeInBytes);

        while (priorityQueue.TryDequeue(out var chunkReader, out var minValue))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await sortedFileWriter.Writer.WriteLineAsync(minValue.Value);
            string nextMinValue;

            if ((nextMinValue = await chunkReader.Reader.ReadLineAsync(cancellationToken)) is not null)
            {
                priorityQueue.Enqueue(chunkReader, new TextLine(nextMinValue));
            }
            else
            {
                // end of chunk
                chunkReader.Dispose();
            }
        }

        await sortedFileWriter.Writer.FlushAsync(cancellationToken);

        if (sortedFileWriter.Stream.Length > sortedFileWriter.Stream.Position)
        {
            sortedFileWriter.Stream.SetLength(sortedFileWriter.Stream.Position);
        }

        Console.WriteLine("Merge sort end");
    }
}
