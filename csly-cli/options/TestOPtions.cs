using CommandLine;

namespace sly.cli.options;

[Verb("test")]
public class TestOPtions
{
    [Option('g',"grammar",Required = true,HelpText = "path to grammar file")]
    public string Grammar { get; set; }
    
    [Option('s',"source",Required = true,HelpText = "path to source file")]
    public string Source { get; set; }
    
    [Option('o',"outptut",Required = false,HelpText = "path to output")]
    public string Output { get; set; }
    
    [Option('f',"format",Required = false,HelpText = "format of output file : DOT=grpahviz dot file, JSON=json")]
    public OutputFormat? OUtputType { get; set; }

    public bool HasOtput => !string.IsNullOrEmpty(Output);

}

public enum OutputFormat
{
    JSON = 0,
    DOT = 1
}

[Verb("generate")]
public class GenerateOPtions
{
    [Option('g',"grammar",Required = true,HelpText = "path to grammar file")]
    public string Grammar { get; set; }
    
    [Option('n',"namespace",Required = true,HelpText = "namespace of the generated source")]
    public string NameSpace { get; set; }
    
    [Option('p',"parser",Required = true,HelpText = "paser class name")]
    public string Parser { get; set; }
    
    [Option('l',"lexer",Required = true,HelpText = "Lexer enum name")]
    public string Lexer { get; set; }
    
    [Option('l',"t",Required = true,HelpText = "type of the of parser output")]
    public string OUtputType { get; set; }
    
}