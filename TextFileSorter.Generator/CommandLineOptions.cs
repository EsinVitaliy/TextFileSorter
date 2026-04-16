using CommandLine;

namespace TextFileSorter.Generator;

internal class CommandLineOptions
{
    [Option('p', "file-path", Required = true, HelpText = "Path to the generated file")]
    public string FilePath { get; set; }

    [Option('s', "file-size", Required = true, HelpText = "File size in megabytes")]
    public int FileSizeInMegabytes { get; set; }
}
