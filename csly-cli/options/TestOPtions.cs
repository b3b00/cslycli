using System.Collections.Generic;
using CommandLine;

namespace sly.cli.options;

[Verb("test")]
public class TestOptions
{
    [Option('g',"grammar",Required = true,HelpText = "path to grammar file")]
    public string Grammar { get; set; }
    
    [Option('s',"source",Required = true,HelpText = "path to source file")]
    public string Source { get; set; }
    
    [Option('o',"outptut",Required = false,HelpText = "path to output")]
    public string Output { get; set; }
    
    [Option('f',"format",Required = false,HelpText = "format of output file : DOT=grpahviz dot file, JSON=json")]
    public IEnumerable<OutputFormat?> OutputTypes { get; set; }

    public bool HasOtput => !string.IsNullOrEmpty(Output);

}

public enum OutputFormat
{
    NO = 0,
    JSON = 1,
    DOT = 2
}