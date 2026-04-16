using System.Text;
using TextFileSorter.Models;

namespace TextFileSorter.Factories;

internal static class FileProcessFactory
{
    private const int DefaultFileStreamBufferSize = 4096;
    private const int DefaultStreamReaderBufferSize = 1024;
    private const int OneMegabyteInBytes = 1024 * 1024;

    public static FileReader CreateFileReader(
        string filePath,
        bool adaptBufferSizesByFileSize = false)
    {
        int fileStreamBufferSize = adaptBufferSizesByFileSize
            ? CalculateFileStreamBufferSize(filePath)
            : DefaultFileStreamBufferSize;

        int streamReaderBufferSize = adaptBufferSizesByFileSize
            ? CalculateStreamReaderBufferSize(fileStreamBufferSize)
            : DefaultStreamReaderBufferSize;

        FileStream fileStream = new(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None,
            fileStreamBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return new FileReader
        {
            Stream = fileStream,
            Reader = new(fileStream, bufferSize: streamReaderBufferSize, leaveOpen: true)
        };
    }

    public static FileWriter CreateFileWriter(string filePath, long approximateFileSizeInBytes = 0)
    {
        FileStream fileStream = new(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            DefaultFileStreamBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (approximateFileSizeInBytes > 0)
        {
            fileStream.SetLength(approximateFileSizeInBytes);
        }

        return new FileWriter
        {
            Stream = fileStream,
            Writer = new(fileStream, leaveOpen: true)
        };
    }

    private static int CalculateFileStreamBufferSize(string filePath)
    {
        const int oneHundredMegabytes = 100 * 1024 * 1024;
        const int fiveHundredMegabytes = 500 * 1024 * 1024;
        const int oneGigabyte = 1024 * 1024 * 1024;

        FileInfo fileInfo = new(filePath);

        switch (fileInfo.Length)
        {
            case < OneMegabyteInBytes:
                return DefaultFileStreamBufferSize;

            case < oneHundredMegabytes:
                return 64 * 1024;

            case < fiveHundredMegabytes:
                return 256 * 1024;

            case < oneGigabyte:
                return 512 * 1024;

            case >= oneGigabyte:
                return 1024 * 1024;
        }
    }

    private static int CalculateStreamReaderBufferSize(int fileStreamBufferSize)
    {
        int bytesPerChar = Encoding.UTF8.GetMaxByteCount(1);
        int bufferSize = fileStreamBufferSize / bytesPerChar;
        return Math.Max(bufferSize, 1024);
    }
}
