using System.Diagnostics;
using sly.lexer;

namespace csly.cli.parser;

[Lexer(IgnoreEOL = true, IgnoreWS = true)]
public enum CLIToken
{
    EOS = 0,

    [Sugar(";")]
    SEMICOLON = 1,
    
    [Int] INT,
    
    [String("'","'")] STRING,

    [SingleLineComment("#")]
    
    [MultiLineComment("/*","*/")]
    COMMENT,

    [Sugar("[")] LEFTBRACKET,
    [Sugar("]")] RIGHTBRACKET,
    [Sugar("(")] LEFTPAREN,
    [Sugar(")")] RIGHTPAREN,
    [Sugar(":")] COLON,
    [Sugar("@")] ROOT,
    
    [Keyword("genericLexer")]
    GENERICLEXER,
    [Keyword("parser")]
    PARSER,
    
    
    [Keyword("String")] STRINGTOKEN,
    [Keyword("Int")] INTTOKEN,
    [Keyword("Double")] DOUBLETOKEN,
    [Keyword("AlphaId")] ALPHAIDTOKEN,
    [Keyword("AlphaNumId")] ALPHANUMIDTOKEN,
    [Keyword("AlphaNumDashId")] ALPHANUMDASHIDTOKEN,
    [Keyword("KeyWord")] KEYWORDTOKEN,
    [Keyword("Sugar")] SUGARTOKEN,
    [Keyword("SingleLineComment")] SINGLELINECOMMENT,
    [Keyword("MultiLineComment")] MULTILINECOMMENT,
    
    
    
    
    [AlphaNumDashId] 
    ID,
        
    
        
    [Sugar("*")]
    ZEROORMORE,
        
    [Sugar("+")]
    ONEORMORE,
        
    [Sugar("?")]
    OPTION ,
        
    [Sugar("[d]")]
    DISCARD,
        
   
        
    [Sugar("|")]
    OR,
    
    [Keyword("Right")] RIGHT,
    [Keyword("Left")] LEFT,
    [Keyword("Operand")] OPERAND,
    [Keyword("Prefix")] PREFIX,
    
    
}