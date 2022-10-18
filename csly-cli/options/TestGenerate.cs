using CommandLine;

namespace sly.cli.options;


[Verb("testgen")]
public class TestGenerate
{
    public string Grammar { get; set; }
    
    [Option('s',"source",Required = true,HelpText = "path to source file")]
    public string Source { get; set; } 
}