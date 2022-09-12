using System.Diagnostics;
using sly.lexer;

namespace CslyCliParser;

[Lexer(IgnoreEOL = true, IgnoreWS = true)]
public enum CLIToken
{

    BLOP = 0,
    
    [Mode("default","genericLexer")]
    [String("'","\\")] STRING,

    [Mode("default","genericLexer")]
    [SingleLineComment("#")]
    COMMENT,
    
    // [Mode("default","genericLexer")]
    // [Lexeme(GenericToken.SugarToken,"\r",IsLineEnding = true)]
    // [Mode("default","genericLexer")]
    // [Sugar("\n")]
    // [Mode("default","genericLexer")]
    // [Sugar("\r\n")]
    // EOL,
    
    [Mode("genericLexer")]
    [Sugar("[")] LEFTBRACKET,
    [Mode("genericLexer")]
    [Sugar("]")] RIGHTBRACKET,
    [Mode("genericLexer")]
    [Sugar("(")] LEFTPAREN,
    [Mode("genericLexer")]
    [Sugar(")")] RIGHTPAREN,
    [Mode("genericLexer")]
    [Sugar(":")] COLON,
    
    [Keyword("genericLexer")]
    [Push("genericLexer")]
    GENERICLEXER,
    
    [Mode("genericLexer")]
    [Keyword("String")] STRINGTOKEN,
    
    [Mode("genericLexer")]
    [Keyword("Int")] INTTOKEN,
    
    [Mode("genericLexer")]
    [Keyword("Double")] DOUBLETOKEN,
    
    [Mode("genericLexer")]
    [Keyword("AlphaId")] ALPHAIDTOKEN,
    
    [Mode("genericLexer")]
    [Keyword("KeyWord")] KEYWORDTOKEN,
    
    [Mode("genericLexer")]
    [Keyword("Sugar")] SUGARTOKEN,
    
    [AlphaId]
    ID,
    
    // [Keyword("parser")]
    // [Push("parser")]
    
    
    
}