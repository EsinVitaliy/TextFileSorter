using CommandLine;

namespace TextFileSorter.Generator;

internal class CommandLineOptions
{
    [Option('o', "output", Required = true, HelpText = "Path to output file")]
    public string FilePath { get; set; }

    [Option('s', "size", Required = true, HelpText = "Size for output file, in megabytes")]
    public int FileSizeInMegabytes { get; set; }
}
