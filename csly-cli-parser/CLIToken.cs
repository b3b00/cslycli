using System.Diagnostics;
using sly.lexer;

namespace csly.cli.parser;

[Lexer(IgnoreEOL = true, IgnoreWS = true)]
public enum CLIToken
{
    EOS = 0,

    [Sugar(";")]
    SEMICOLON = 1,
    
    [Mode("default","EXT")]
    [Int] INT,
    
    [String("'","'")] STRING,

    // [Mode]
    // [SingleLineComment("#")]
    //
    // [Mode]
    // [MultiLineComment("/*","*/")]
    // COMMENT,

    [Mode("default","EXT")]
    [Sugar("[")] LEFTBRACKET,
    [Mode("default","EXT")]
    [Sugar("]")] RIGHTBRACKET,
    
    
    [Sugar("(")] LEFTPAREN,
    [Sugar(")")] RIGHTPAREN,
    [Mode("default","EXT")]
    [Sugar(":")] COLON,
    [Sugar("->")] ROOT,
    [Sugar("^")] NOT,
    
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
    [Keyword("Extension")] EXTENSIONTOKEN,
    
    
    
    
    [AlphaNumDashId] 
    [Mode("default","EXT")]
    ID,
        
    
    [Mode("EXT")]
    [Push("RANGE")]
    [Sugar("[[")] LEFTBRACKETBRACKET,
    [Mode("EXT")]
    [Pop]
    [Mode("RANGE")]
    [Sugar("]]")] RIGHTBRACKETBRACKET,
    
    [Mode("RANGE")]
    [Extension]
    RANGE,
    
        
    [Sugar("*")]
    [Mode("default","EXT")]
    ZEROORMORE,
        
    [Sugar("+")]
    [Mode("default","EXT")]
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
    [Keyword("Postfix")] POSTFIX,
    
    [Push("EXT")]
    [Mode]
    [Sugar(">>>")]
    OPEN_EXT,
    
    [Mode("EXT")]
    [Sugar("->")]
    ARROW,
    
    [Mode("EXT")]
    [Sugar("@")]
    AT,
    
    [Mode("EXT")]
    [Sugar("-")]
    DASH,
    
    
    [Mode("EXT")]
    [Sugar("{")]
    LEFTCURL,
    
    [Mode("EXT")]
    [Sugar("}")]
    RIGHTCURL,
    
    [Mode("EXT","RANGE")]
    [Sugar(",")]
    COMMA,
    
    [Mode("EXT")]
    [Keyword("END")]
    END,
    
    
    [Pop]
    [Mode("EXT")]
    [Sugar("<<<")]
    CLOSE_EXT,
    
    [Extension]
    [Mode("EXT")]
    CHAR
}