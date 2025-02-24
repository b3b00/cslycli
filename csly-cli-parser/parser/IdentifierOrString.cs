using csly.cli.model;
using csly.cli.parser;
using sly.lexer;

namespace clsy.cli.model.parser;

public class IdentifierOrString : ICLIModel
{

    public bool IsString => TokenId == CLIToken.STRING; 

    public string Value => Token.Value;
    
    public LexerPosition Position
    {
        get { return Token.Position; }
        set { }
    }

    public CLIToken TokenId => Token.TokenID;
    
    public Token<CLIToken> Token { get; set; }
    public string StringWithoutQuotes => Token.StringWithoutQuotes;

    public bool IsExplicit => Token.IsExplicit;

    public IdentifierOrString(Token<CLIToken> token)
    {
        Token = token;
    }

    
}