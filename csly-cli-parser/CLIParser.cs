using csly_cli_model;
using sly.lexer;
using sly.parser.generator;

namespace csly.cli.parser;

public class CLIParser
{
    [Production("root: generic")] 
    public ICLIModel Root(ICLIModel genericLex)
    {
        return genericLex;
    }
    
    
    [Production("generic : GENERICLEXER[d]  token*")]
    public ICLIModel lexer(List<ICLIModel> tokens)
    {
        return new LexerModel(tokens.Cast<TokenModel>().ToList());
    }

    [Production(
        "token :LEFTBRACKET[d] [KEYWORDTOKEN|SUGARTOKEN] RIGHTBRACKET[d] ID COLON[d] STRING")]
    public ICLIModel Token(Token<CLIToken> type, Token<CLIToken> id, Token<CLIToken> value)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.KEYWORDTOKEN => GenericToken.KeyWord,
            CLIToken.SUGARTOKEN => GenericToken.SugarToken,
            _ => GenericToken.SugarToken
        };
        return new TokenModel(tokenType,id.Value,value.StringWithoutQuotes);
    }

    [Production("token : LEFTBRACKET[d] [STRINGTOKEN|INTTOKEN|ALPHAIDTOKEN|DOUBLETOKEN] RIGHTBRACKET[d] ID")]
    public ICLIModel StringToken(Token<CLIToken> type, Token<CLIToken> id)
    {
        var tokenType = type.TokenID switch
        {
            CLIToken.STRINGTOKEN => GenericToken.String,
            CLIToken.INTTOKEN => GenericToken.Int,
            CLIToken.DOUBLETOKEN => GenericToken.Double,
            CLIToken.ALPHAIDTOKEN => GenericToken.Identifier,
            _ => GenericToken.SugarToken
        };
        if (type.TokenID == CLIToken.STRINGTOKEN)
        {
            return new TokenModel(tokenType, id.Value,"\"", "\\");
        }
        return new TokenModel(tokenType,id.Value, "");
    } 
    
  
        
}