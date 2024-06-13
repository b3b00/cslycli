using CommandLine;

namespace sly.cli.options;

[Verb("project")]
public class GenerateProjectOptions
{
    [Option('g',"grammar",Required = true,HelpText = "path to grammar file")]
    public string Grammar { get; set; }
    
    [Option('n',"namespace",Required = true,HelpText = "namespace of the generated source")] 
    public string NameSpace { get; set; } = "ns";

    [Option('o', "parserOutput", Required = false, HelpText = "parser output type")]
    public string ParserOutput { get; set; } = "object";
    
    [Option('d', "output-dir", Required = false, HelpText = "output directory")]
    public string OutputDir { get; set; } = "./";

    public GenerateOptions ToGenerateOptions()
    {
        return new GenerateOptions()
        {
            Grammar = Grammar,
            NameSpace = NameSpace,
            OutputDir = OutputDir,
            ParserOutput = ParserOutput
        };
    }
    
}