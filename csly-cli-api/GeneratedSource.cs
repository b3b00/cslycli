namespace csly_cli_api;

public class GeneratedSource
{
    
    public string LexerName { get; set; }
    public string Lexer { get; set; }

    public string ParserName { get; set; }
    public string Parser { get; set; }
    
    public string Project { get; set; }
    
    public string Program { get; set; }

    public GeneratedSource(string lexerName, string lexer, string parserName, string parser, string project, string program)
    {
        LexerName = lexerName;
        ParserName = parserName;
        Lexer = lexer;
        Parser = parser;
        Project = project;
        Program = program;
    }
}

