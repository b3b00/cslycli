using csly.cli.model;
using sly.lexer;

namespace clsy.cli.model.parser;

public class Identifier: ICLIModel
{
    
    public string Name { get; set; }
    
    public LexerPosition Position { get; set; }
    
    public Identifier(string name, CLIToken tokenid, LexerPosition position)
    {
        Name = name;
        Position = position;
    }
}

public class IdentifierOrString : ICLIModel
{
    public string Name { get; set; }
    
    public LexerPosition Position { get; set; }
    
    public bool IsString { get; set; }
    public IdentifierOrString(string name, bool isString, LexerPosition position)
    {
        Name = name;
        Position = position;
        IsString = isString;
    }

    
}