using System.Diagnostics;
using sly.lexer;

namespace CslyCliParser;

[Lexer(IgnoreEOL = true, IgnoreWS = true)]
public enum CLIToken
{
    
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
    [Keyword("String")] STRING_TOKEN,
    
    [Mode("genericLexer")]
    [Keyword("Int")] INT_TOKEN,
    
    [Mode("genericLexer")]
    [Keyword("Double")] DOUBLE_TOKEN,
    
    [Mode("genericLexer")]
    [Keyword("AlphaId")] ALPHA_ID_TOKEN,
    
    [Mode("genericLexer")]
    [Keyword("KeyWord")] KEYWORD_TOKEN,
    
    [Mode("genericLexer")]
    [Keyword("Sugar")] SUGAR_TOKEN,
    
    [AlphaId]
    ID,
    
    // [Keyword("parser")]
    // [Push("parser")]
    
    
    
}