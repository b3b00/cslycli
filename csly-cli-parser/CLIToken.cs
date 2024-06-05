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
    
    [String("\"","\\")] STRING,


    [Mode("EXT","default")]
    [SingleLineComment("#")]
    
    [Mode]
    [MultiLineComment("/*","*/")]
    COMMENT,


    [Mode("default","EXT")]
    [Sugar("[")] LEFTBRACKET,
    [Mode("default","EXT")]
    [Sugar("]")] RIGHTBRACKET,
    
    
    [Mode("EXT","default")]
    [Sugar("(")] LEFTPAREN,
    [Mode("EXT","default")]
    [Sugar(")")] RIGHTPAREN,
    [Mode("default","EXT")]
    [Sugar(":")] COLON,
    [Sugar("^")] NOT,
    
    [Keyword("genericLexer")]
    GENERICLEXER,
    [Keyword("parser")]
    PARSER,
    
    
    [Keyword("String")] STRINGTOKEN,
    [Keyword("Character")] CHARTOKEN,
    [Keyword("Int")] INTTOKEN,
    [Keyword("Date")] DATETOKEN,
    [Keyword("Double")] DOUBLETOKEN,
    [Keyword("AlphaId")] ALPHAIDTOKEN,
    [Keyword("AlphaNumId")] ALPHANUMIDTOKEN,
    [Keyword("AlphaNumDashId")] ALPHANUMDASHIDTOKEN,
    [Keyword("KeyWord")] KEYWORDTOKEN,
    [Keyword("Sugar")] SUGARTOKEN,
    [Keyword("SingleLineComment")] SINGLELINECOMMENT,
    [Keyword("MultiLineComment")] MULTILINECOMMENT,
    [Keyword("Extension")] EXTENSIONTOKEN,
    [Keyword("UpTo")] UPTOTOKEN,
    [Keyword("Push")] PUSH,
    [Keyword("Mode")] MODE,
    [Keyword("Pop")] POP,
    [Keyword("true")] TRUE,
    [Keyword("false")] FALSE,
    
    
    // parser optimizations
    [Keyword("UseMemoization")] USEMEMOIZATION,
    [Keyword("BroadenTokenWindow")] BROADENTOKENWINDOW,
    
    // lexer options
    [Keyword("IndentationAware")] INDENTATIONAWARE,
    [Keyword("IgnoreWhiteSpaces")] IGNOREWHITESPACES,
    [Keyword("IgnoreEndOfLines")] IGNOREEOL,
    [Keyword("IgnoreKeyWordCase")] IGNOREKEYWORDCASING,
    
    [Keyword("YYYYMMDD")] YYYYMMDD,
    [Keyword("DDMMYYYY")] DDMMYYYY,

    
    
    
    
    
    [AlphaNumDashId] 
    [Mode("default","EXT")]
    ID,
    
    [Mode("default","EXT")]
    [Character("'","\\")]
    CHAR,
        
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
    
    [Mode("EXT","default")]
    [Sugar("->")]
    ARROW,
    
    [Mode("EXT","default")]
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
    
    [Mode("default","EXT")]
    [Sugar(",")]
    COMMA,
    
    [Mode("EXT")]
    [Keyword("END")]
    ENDTOKEN,
    
    
    [Pop]
    [Mode("EXT")]
    [Sugar("<<<")]
    CLOSE_EXT,
    
   
}