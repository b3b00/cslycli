namespace csly_cli_api;

public class GeneratedSource
{
    
    public string LexerName { get; set; }
    public string Lexer { get; set; }

    public string ParserName { get; set; }
    public string Parser { get; set; }
    
    public string Project { get; set; }

    public GeneratedSource(string lexerName, string lexer, string parserName, string parser, string project)
    {
        LexerName = lexerName;
        ParserName = parserName;
        Lexer = lexer;
        Parser = parser;
        Project = project;
    }
}