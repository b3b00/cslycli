using CommandLine;

namespace sly.cli.options;

[Verb("generate")]
public class GenerateOPtions
{
    [Option('g',"grammar",Required = true,HelpText = "path to grammar file")]
    public string Grammar { get; set; }
    
    [Option('n',"namespace",Required = true,HelpText = "namespace of the generated source")] 
    public string NameSpace { get; set; } = "ns";

    [Option('p', "parser", Required = true, HelpText = "paser class name")]
    public string Parser { get; set; } = "Parser";

    [Option('l', "lexer", Required = true, HelpText = "Lexer enum name")]
    public string Lexer { get; set; } = "Lexer";

    [Option('o', "parserOutput", Required = false, HelpText = "parser output type")]
    public string ParserOutput { get; set; } = "object";

}