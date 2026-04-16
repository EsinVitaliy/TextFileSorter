using CommandLine;
using System.Text;

namespace TextFileSorter.Generator;

internal class Program
{
    public static readonly string FirstTextLineForUnsortedFile = $"9999999999.{new string('z', 50)}";
    public static readonly string LastTextLineForUnsortedFile = "0000000000.0";

    static async Task Main(string[] args)
    {
        string filePath = null;
        int fileSizeInMegabytes = 0;

        Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(o =>
            {
                if (string.IsNullOrWhiteSpace(o.FilePath))
                {
                    Console.Error.WriteLine("Error: file path is required");
                    return;
                }

                if (o.FileSizeInMegabytes <= 0)
                {
                    Console.Error.WriteLine("Error: file size must be a positive integer");
                    return;
                }

                filePath = o.FilePath;
                fileSizeInMegabytes = o.FileSizeInMegabytes;
            });

        if (string.IsNullOrWhiteSpace(filePath) || fileSizeInMegabytes <= 0)
        {
            Environment.Exit(1);
        }

        Console.WriteLine("Begin creating file");
        Console.WriteLine($"Path = {filePath}");
        Console.WriteLine($"Size = {fileSizeInMegabytes} MB");

        await GenerateFile(filePath, fileSizeInMegabytes);

        Console.WriteLine("File is created");
    }

    internal static async Task GenerateFile(string filePath, long sizeInMegabytes)
    {
        await using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using StreamWriter writer = new(fileStream, leaveOpen: true);

        long maxSizeInBytes = sizeInMegabytes * 1024 * 1024;

        long fileSizeInBytes = Encoding.UTF8.GetByteCount(FirstTextLineForUnsortedFile + Environment.NewLine);
        await writer.WriteLineAsync(FirstTextLineForUnsortedFile);

        while (true)
        {
            string textLine = GenerateTextLine();
            long byteCount = Encoding.UTF8.GetByteCount(textLine + Environment.NewLine);

            if (fileSizeInBytes + byteCount > maxSizeInBytes)
            {
                await writer.WriteLineAsync(LastTextLineForUnsortedFile);
                break;
            }

            fileSizeInBytes += byteCount;
            await writer.WriteLineAsync(textLine);
        }
    }

    internal static string GenerateTextLine()
    {
        int randomNumber = Random.Shared.Next();
        int randomStringLength = Random.Shared.Next(1, 50);

        return randomNumber + "." + GenerateRandomString(randomStringLength);
    }

    internal static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[Random.Shared.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}
