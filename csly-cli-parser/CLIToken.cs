using System.Diagnostics;
using sly.lexer;

namespace csly.cli.parser;

[Lexer(IgnoreEOL = true, IgnoreWS = true)]
public enum CLIToken
{

    BLOP = 0,
    
    [Int] INT,
    
    [String("'","\\")] STRING,

    [SingleLineComment("#")]
    COMMENT,

    [Sugar("[")] LEFTBRACKET,
    [Sugar("]")] RIGHTBRACKET,
    [Sugar("(")] LEFTPAREN,
    [Sugar(")")] RIGHTPAREN,
    [Sugar(":")] COLON,
    
    [Keyword("genericLexer")]
    GENERICLEXER,
    
    [Keyword("String")] STRINGTOKEN,
    [Keyword("Int")] INTTOKEN,
    [Keyword("Double")] DOUBLETOKEN,
    [Keyword("AlphaId")] ALPHAIDTOKEN,
    [Keyword("KeyWord")] KEYWORDTOKEN,
    [Keyword("Sugar")] SUGARTOKEN,
    
    
    
    
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