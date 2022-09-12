using System.Security.Cryptography;
using sly.lexer;
using sly.parser.generator;

namespace CslyCliParser;

public class ClIParser
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
        "token :LEFTBRACKET[d] [|ALPHA_ID_TOKEN|KEYWORD_TOKEN|SUGAR_TOKEN] RIGHTBRACKET[d] ID COLON[d] STRING")]
    public object Token(Token<CLIToken> type, Token<CLIToken> id, Token<CLIToken> value)
    {
        Console.WriteLine($"[{type.TokenID}] {id.Value} = '{value.Value.Trim()}'");
        return null;
    }

    [Production("token : LEFTBRACKET[d] [STRING_TOKEN|INT_TOKEN|ALPHA_ID_TOKEN|DOUBLE_TOKEN] RIGHTBRACKET[d] ID")]
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