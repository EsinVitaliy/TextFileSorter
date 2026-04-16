using CommandLine;
using Humanizer;
using TextFileSorter.Interfaces.ProcessStages;
using TextFileSorter.ProcessStages;

namespace TextFileSorter;

internal class Program
{
    private const string ChunkFileExtension = ".chunk";
    private static CancellationTokenSource _cancellationTokenSource;

    static async Task Main(string[] args)
    {
        var arguments = ParseArguments(args);
        SubscribeConsoleEvents();
        _cancellationTokenSource = new();

        try
        {
            await RunAsync(
                arguments.InputFilePath,
                arguments.OutputFilePath,
                arguments.MaxChunkSizeInMegabytes,
                _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
    
    internal static async Task RunAsync(
        string inputFilePath,
        string outputFilePath,
        int maxChunkSizeInMegabytes,
        CancellationToken cancellationToken)
    {
        long inputFileSizeInBytes = GetFileSizeInBytes(inputFilePath);
        long maxChunkSizeInBytes;

        if (maxChunkSizeInMegabytes <= 0)
        {
            maxChunkSizeInBytes = GetChunkSizeInBytes(inputFileSizeInBytes);
        }
        else
        {
            maxChunkSizeInBytes = Math.Clamp(maxChunkSizeInMegabytes, 1, 1024) * 1024 * 1024;
        }

        int reservedMemoryInMegabytes = CalculateReservedMemoryInMegabytes(maxChunkSizeInBytes);

        Console.WriteLine($"Input file = {inputFilePath}");
        Console.WriteLine($"Output file = {outputFilePath}");
        Console.WriteLine($"Input file size = {inputFileSizeInBytes.Bytes().Humanize()}");
        Console.WriteLine($"Chunk size = {maxChunkSizeInBytes.Bytes().Humanize()}");
        Console.WriteLine($"Reserved memory = {reservedMemoryInMegabytes.Megabytes().Humanize()}");
        Console.WriteLine("---");

        string chunkDirectoryPath = CreateTempDirectory(inputFilePath);

        try
        {
            IChunkProducer chunkProducer = new ChunkProducer(inputFilePath, maxChunkSizeInBytes);
            IChunkConsumer chunkConsumer = new ChunkConsumer(chunkDirectoryPath, ChunkFileExtension);

            IChunkMergeSorter chunkMergeSorter = new KWayMergeSorter(
                chunkDirectoryPath,
                ChunkFileExtension,
                outputFilePath,
                inputFileSizeInBytes);

            TextProcessor textProcessor = new(chunkProducer, chunkConsumer, chunkMergeSorter, reservedMemoryInMegabytes);
            await textProcessor.ProcessAsync(cancellationToken);
        }
        finally
        {
            DeleteTempDirectory(chunkDirectoryPath);
        }
    }

    private static (string InputFilePath, string OutputFilePath, int MaxChunkSizeInMegabytes) ParseArguments(string[] args)
    {
        string inputFilePath = null;
        string outputFilePath = null;
        int maxChunkSizeInMegabytes = 0;

        Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(o =>
            {
                if (string.IsNullOrWhiteSpace(o.InputFilePath))
                {
                    Console.Error.WriteLine("Error: input file path is required");
                    return;
                }

                if (o.MaxChunkSizeInMegabytes.HasValue && o.MaxChunkSizeInMegabytes <= 0)
                {
                    Console.Error.WriteLine("Error: chunk size must be a positive integer");
                    return;
                }

                inputFilePath = o.InputFilePath;

                outputFilePath = string.IsNullOrWhiteSpace(o.OutputFilePath)
                    ? inputFilePath
                    : o.OutputFilePath;

                maxChunkSizeInMegabytes = o.MaxChunkSizeInMegabytes ?? 0;
            });

        return (inputFilePath, outputFilePath, maxChunkSizeInMegabytes);
    }

    private static void SubscribeConsoleEvents()
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Abort();
        };
    }

    private static int CalculateReservedMemoryInMegabytes(long maxChunkSizeInBytes)
    {
        long chunkReservedMemoryInBytes = maxChunkSizeInBytes * 2;

        // 30% of the chunk reserved memory is added as a safety margin to prevent OutOfMemoryException during processing
        long safetyMarginInBytes = chunkReservedMemoryInBytes / 100 * 30;

        long reservedMemoryInMegabytesAsLong = (chunkReservedMemoryInBytes + safetyMarginInBytes) / 1024 / 1024;
        return (int)Math.Clamp(reservedMemoryInMegabytesAsLong, 1, int.MaxValue);
    }

    private static long GetFileSizeInBytes(string filePath)
    {
        FileInfo fileInfo = new(filePath);
        return fileInfo.Length;
    }
    
    private static long GetChunkSizeInBytes(long fileSizeInBytes)
    {
        const long oneMegabyte = 1024 * 1024;
        const long oneGigabyte = 1024 * oneMegabyte;

        const long desiredChunkCount = 8;
        const long minChunkSize = desiredChunkCount * oneMegabyte;

        if (fileSizeInBytes < minChunkSize)
        {
            return fileSizeInBytes;
        }

        if (fileSizeInBytes <= oneGigabyte)
        {
            return fileSizeInBytes / desiredChunkCount;
        }

        return oneGigabyte;
    }

    private static string CreateTempDirectory(string inputFilePath)
    {
        string tempDirectoryName = Path.GetFileName(inputFilePath) + "_chunks.$$$";
        string tempDirectoryPath = Path.Join(Path.GetDirectoryName(inputFilePath), tempDirectoryName);
        Directory.CreateDirectory(tempDirectoryPath);

        return tempDirectoryPath;
    }

    private static void DeleteTempDirectory(string tempDirectoryPath)
    {
        string[] chunkPaths = Directory.GetFiles(tempDirectoryPath, "*" + ChunkFileExtension);

        foreach (var chunkPath in chunkPaths)
        {
            File.Delete(chunkPath);
        }

        Directory.Delete(tempDirectoryPath);
    }

    private static void Abort()
    {
        Console.WriteLine("Abort all operations");
        _cancellationTokenSource.Cancel();
    }
}
