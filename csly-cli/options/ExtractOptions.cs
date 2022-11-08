using CommandLine;

namespace sly.cli.options;

[Verb("extract")]
public class ExtractOptions
{
    [Option('l',"lexer",Required = true,HelpText = "path to lexer C# file")]
    public string LexerPath { get; set; }
    
    [Option('p',"parser",Required = true,HelpText = "path to parser C# file")] 
    public string ParserPath { get; set; }

    [Option('o', "output", Required = true, HelpText = "specification output file")]
    public string SpecificationOutputFile { get; set; } = "./";

}

[Verb("decompile")]
public class DecompileOptions
{
    [Option('l',"lexer",Required = true,HelpText = "lexer fully qualified name.")]
    public string LexerFqn { get; set; }
    
    [Option('p',"parser",Required = true,HelpText = "parser fully qualified name.")] 
    public string ParserFqn { get; set; }
    
    [Option('a',"assembly",Required = true,HelpText = "path to assembly")] 
    public string AssemblyPath { get; set; }

    [Option('o', "output", Required = true, HelpText = "specification output file")]
    public string SpecificationOutputFile { get; set; } = "./";

}