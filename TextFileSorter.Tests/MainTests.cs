using AwesomeAssertions;

namespace TextFileSorter.Tests;

public class MainTests
{
    private static readonly string GeneratedFilePath;
    private static readonly string SortedFilePath;

    static MainTests()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        GeneratedFilePath = Path.Join(basePath, "generated.txt");
        SortedFilePath = GeneratedFilePath + "_sorted.txt";
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(3 * 1024, 1024)]
    [InlineData(100 * 1024, 0)]
    public async Task T01_should_sort_file(int generatedFileSizeInMegabytes, int maxChunkSizeInMegabytes)
    {
        await Generator.Program.GenerateFile(GeneratedFilePath, generatedFileSizeInMegabytes);

        File.Exists(GeneratedFilePath)
            .Should().BeTrue();

        await Program.RunAsync(GeneratedFilePath, SortedFilePath, maxChunkSizeInMegabytes, TestContext.Current.CancellationToken);

        var fileData = await GetFirstAndLastTextLines(SortedFilePath);

        fileData.FirstTextLine
            .Should().BeEquivalentTo(Generator.Program.LastTextLineForUnsortedFile);

        fileData.LastTextLine
            .Should().BeEquivalentTo(Generator.Program.FirstTextLineForUnsortedFile);

        if (generatedFileSizeInMegabytes <= 2048)
        {
            await CompareTwoFiles(GeneratedFilePath, SortedFilePath);
        }
    }

    private async Task<(string FirstTextLine, string LastTextLine)> GetFirstAndLastTextLines(string filePath)
    {
        await using FileStream stream = new FileStream(
            filePath, FileMode.Open,
            FileAccess.Read,
            FileShare.None,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        using StreamReader reader = new(stream, leaveOpen: true);

        string firstTextLine = await reader.ReadLineAsync();

        string lastTextLine = "";
        string textLine;

        while ((textLine = await reader.ReadLineAsync()) is not null)
        {
            lastTextLine = textLine;
        }

        return new(firstTextLine, lastTextLine);
    }

    private async Task CompareTwoFiles(string originalFilePath, string sortedFilePath)
    {
        HashSet<string> originalFileTextLines = [];
        string textLine;

        using (StreamReader originalFileReader = new(originalFilePath))
        {
            while ((textLine = await originalFileReader.ReadLineAsync()) is not null)
            {
                originalFileTextLines.Add(textLine);
            }
        }

        using StreamReader sortedFileReader = new(sortedFilePath);
        int sortedFileLineCount = 0;

        while ((textLine = await sortedFileReader.ReadLineAsync()) is not null)
        {
            sortedFileLineCount++;

            originalFileTextLines.Contains(textLine)
                .Should().BeTrue();
        }

        originalFileTextLines.Count
            .Should().Be(sortedFileLineCount);
    }
}
