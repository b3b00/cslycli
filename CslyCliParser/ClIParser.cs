using sly.lexer;
using sly.parser.generator;

namespace CslyCliParser;

public class CLIParser
{
    [Production("root: generic")] 
    public object Root(object genericLex)
    {
        return genericLex;
    }
    
    
    [Production("generic : GENERICLEXER[d]  token*")]
    public object lexer(List<object> tokens)
    {
        return null;
    }

    [Production(
        "token :LEFTBRACKET[d] [KEYWORDTOKEN|SUGARTOKEN] RIGHTBRACKET[d] ID COLON[d] STRING")]
    public object Token(Token<CLIToken> type, Token<CLIToken> id, Token<CLIToken> value)
    {
        Console.WriteLine($"[{type.TokenID}] {id.Value} = '{value.StringWithoutQuotes.Trim()}'");
        return null;
    }

    [Production("token : LEFTBRACKET[d] [STRINGTOKEN|INTTOKEN|ALPHAIDTOKEN|DOUBLETOKEN] RIGHTBRACKET[d] ID")]
    public object StringToken(Token<CLIToken> type, Token<CLIToken> id)
    {
        Console.WriteLine($"[{type.Value}] {id.Value}");
        return null;
    } 
    
  
        

    // [Production("token: EOL[d]")]
    // public object EmptyLine()
    // {
    //     return null;
    // }
    //
}