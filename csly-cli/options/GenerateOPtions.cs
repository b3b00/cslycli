using CommandLine;

namespace sly.cli.options;

[Verb("generate")]
public class GenerateOptions
{
    [Option('g',"grammar",Required = true,HelpText = "path to grammar file")]
    public string Grammar { get; set; }
    
    [Option('n',"namespace",Required = true,HelpText = "namespace of the generated source")] 
    public string NameSpace { get; set; } = "ns";

    [Option('o', "parserOutput", Required = false, HelpText = "parser output type")]
    public string ParserOutput { get; set; } = "object";

}