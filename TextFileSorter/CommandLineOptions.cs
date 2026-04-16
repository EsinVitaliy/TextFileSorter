using CommandLine;

namespace TextFileSorter;

internal class CommandLineOptions
{
    [Option('i', "input", Required = true, HelpText = "Path to input file")]
    public string InputFilePath { get; set; }

    [Option('o', "output", Required = false, HelpText = "Path to output sorted file, if omitted the input file will be overwritten")]
    public string OutputFilePath { get; set; }

    [Option('m', "max-chunk-size", Required = false, HelpText = "Maximum chunk size in megabytes, if omitted it will be chosen automatically, maximum allowed value is 1024 MB")]
    public int? MaxChunkSizeInMegabytes { get; set; }
}
