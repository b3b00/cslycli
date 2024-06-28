using System.Diagnostics;
using sly.i18n;
using sly.lexer;

namespace csly.cli.parser;

[Lexer(IgnoreEOL = true, IgnoreWS = true)]
public enum CLIToken
{
    EOS = 0,

    [LexemeLabel("en","semicolon")]
    [LexemeLabel("fr","point-virgule")]
    [Sugar(";")]
    SEMICOLON = 1,
    
    [LexemeLabel("en","integer")]
    [LexemeLabel("fr","entier")]
    [Mode("default","EXT")]
    [Int] INT,
    
    [LexemeLabel("en","string")]
    [LexemeLabel("fr","chaine de caractère")]
    [String("\"","\\")] STRING,


    [LexemeLabel("en","comment")]
    [LexemeLabel("fr","commentaire")]
    [Mode("EXT","default")]
    [SingleLineComment("#")]
    [Mode]
    [MultiLineComment("/*","*/")]
    COMMENT,


    [LexemeLabel("fr","crochet ouvrant")]
    [LexemeLabel("en","opening square bracket")]
    [Mode("default","EXT")]
    [Sugar("[")] LEFTBRACKET,
    [LexemeLabel("fr","crochet fermant")]
    [LexemeLabel("en","closing square bracket")]
    [Mode("default","EXT")]
    [Sugar("]")] RIGHTBRACKET,
    
    
    [LexemeLabel("fr","paranthèse ouvrante")]
    [LexemeLabel("en","opening paranthesis")]
    [Mode("EXT","default")]
    [Sugar("(")] LEFTPAREN,
    
    [LexemeLabel("fr","paranthèse fermante")]
    [LexemeLabel("en","closing paranthesis")]
    [Mode("EXT","default")]
    [Sugar(")")] RIGHTPAREN,
    [Mode("default","EXT")]
    [LexemeLabel("fr","deux point")]
    [LexemeLabel("en","colon")]
    [Sugar(":")] COLON,
    [Sugar("^")] NOT,
    
    [LexemeLabel("fr","mot clé 'genericLexer'")]
    [LexemeLabel("en","'genericLexer' keyword")]
    [Keyword("genericLexer")]
    GENERICLEXER,
    
    [LexemeLabel("fr","mot clé 'parser'")]
    [LexemeLabel("en","'parser' keyword")]
    [Keyword("parser")]
    PARSER,
    
    [LexemeLabel("fr","mot clé 'string'")]
    [LexemeLabel("en","string' keyword")]
    [Keyword("String")] STRINGTOKEN,
    [LexemeLabel("fr","mot clé 'haracter'")]
    [LexemeLabel("en","'Character' keyword")]
    [Keyword("Character")] CHARTOKEN,
    [LexemeLabel("fr","mot clé 'Int'")]
    [LexemeLabel("en","'Int' keyword")]
    [Keyword("Int")] INTTOKEN,
    [LexemeLabel("fr","mot clé 'Date'")]
    [LexemeLabel("en","'Date' keyword")]
    [Keyword("Date")] DATETOKEN,
    [LexemeLabel("fr","mot clé 'Double'")]
    [LexemeLabel("en","'Double' keyword")]
    [Keyword("Double")] DOUBLETOKEN,
    [LexemeLabel("fr","mot clé 'AlphaId'")]
    [LexemeLabel("en","'AlphaId' keyword")]
    [Keyword("AlphaId")] ALPHAIDTOKEN,
    [LexemeLabel("fr","mot clé 'AlphaNumId'")]
    [LexemeLabel("en","'AlphaNumId' keyword")]
    [Keyword("AlphaNumId")] ALPHANUMIDTOKEN,
    [LexemeLabel("fr","mot clé 'AlphaNumDashId'")]
    [LexemeLabel("en","'AlphanumDashId' keyword")]
    [Keyword("AlphaNumDashId")] ALPHANUMDASHIDTOKEN,
    [LexemeLabel("fr","mot clé 'Keyword'")]
    [LexemeLabel("en","'Keyword' keyword")]
    [Keyword("KeyWord")] KEYWORDTOKEN,
    [LexemeLabel("fr","mot clé 'Sugar'")]
    [LexemeLabel("en","'Sugar' keyword")]
    [Keyword("Sugar")] SUGARTOKEN,
    [LexemeLabel("fr","mot clé 'SingleLineComment'")]
    [LexemeLabel("en","'SingleLineComment' keyword")]
    [Keyword("SingleLineComment")] SINGLELINECOMMENT,
    [LexemeLabel("fr","mot clé 'MultiLineComment'")]
    [LexemeLabel("en","'MultiLineComment' keyword")]
    [Keyword("MultiLineComment")] MULTILINECOMMENT,
    [LexemeLabel("fr","mot clé 'Extension'")]
    [LexemeLabel("en","'Extension' keyword")]
    [Keyword("Extension")] EXTENSIONTOKEN,
    [LexemeLabel("fr","mot clé 'UpTo'")]
    [LexemeLabel("en","'UpTo' keyword")]
    [Keyword("UpTo")] UPTOTOKEN,
    [LexemeLabel("fr","mot clé 'Push'")]
    [LexemeLabel("en","'Push' keyword")]
    [Keyword("Push")] PUSH,
    [LexemeLabel("fr","mot clé 'Mode'")]
    [LexemeLabel("en","'Mode' keyword")]
    [Keyword("Mode")] MODE,
    [LexemeLabel("fr","mot clé 'Pop'")]
    [LexemeLabel("en","'Pop' keyword")]
    [Keyword("Pop")] POP,
    [LexemeLabel("fr","mot clé 'true'")]
    [LexemeLabel("en","'true' keyword")]
    [Keyword("true")] TRUE,
    [LexemeLabel("fr","mot clé 'false'")]
    [LexemeLabel("en","'false' keyword")]
    [Keyword("false")] FALSE,
    
    
    // parser optimizations
    [LexemeLabel("fr","mot clé 'UseMemoization'")]
    [LexemeLabel("en","'UseMemoization' keyword")]
    [Keyword("UseMemoization")] USEMEMOIZATION,
    [LexemeLabel("fr","mot clé 'BroadenTokenWindow'")]
    [LexemeLabel("en","'BroadenTokenWindow' keyword")]
    [Keyword("BroadenTokenWindow")] BROADENTOKENWINDOW,
    [LexemeLabel("fr","mot clé 'AutoCloseIndentations'")]
    [LexemeLabel("en","'AutoCloseIndentations' keyword")]
    [Keyword("AutoCloseIndentations")] AUTOCLOSEINDENTATION,
    
    // lexer options
    [LexemeLabel("fr","mot clé 'IndentationAware'")]
    [LexemeLabel("en","'IndentationAware' keyword")]
    [Keyword("IndentationAware")] INDENTATIONAWARE,
    [LexemeLabel("fr","mot clé 'IgnoreWhiteSpaces'")]
    [LexemeLabel("en","'IgnoreWhiteSpaces' keyword")]
    [Keyword("IgnoreWhiteSpaces")] IGNOREWHITESPACES,
    [LexemeLabel("fr","mot clé 'IgnoreEndOfLines'")]
    [LexemeLabel("en","'IgnoreEndOfLines' keyword")]
    [Keyword("IgnoreEndOfLines")] IGNOREEOL,
    [LexemeLabel("fr","mot clé 'IgnoreKeyWordCase'")]
    [LexemeLabel("en","'IgnoreKeyWordCase' keyword")]
    [Keyword("IgnoreKeyWordCase")] IGNOREKEYWORDCASING,
    
    [LexemeLabel("fr","mot clé 'YYYYMMDD'")]
    [LexemeLabel("en","'YYYYMMDD' keyword")]
    [Keyword("YYYYMMDD")] YYYYMMDD,
    [LexemeLabel("fr","mot clé 'DDMMYYYY'")]
    [LexemeLabel("en","'DDMMYYYY' keyword")]
    [Keyword("DDMMYYYY")] DDMMYYYY,

    [LexemeLabel("fr","mot clé 'INDENT'")]
    [LexemeLabel("en","'INDENT' keyword")]
    [Keyword("INDENT")] INDENT,
    [LexemeLabel("fr","mot clé 'UINDENT'")]
    [LexemeLabel("en","'UINDENT' keyword")]
    [Keyword("UINDENT")] UINDENT,
    
    
    
    
    
    
    [AlphaNumDashId] 
    [LexemeLabel("fr","identifiant")]
    [LexemeLabel("en","identifier")]
    [Mode("default","EXT")]
    ID,
    
    [LexemeLabel("fr","caractère")]
    [LexemeLabel("en","character")]
    [Mode("default","EXT")]
    [Character("'","\\")]
    CHAR,
        
    [Sugar("*")]
    [LexemeLabel("fr","zéro ou plus")]
    [LexemeLabel("en","zero or more")]
    [Mode("default","EXT")]
    ZEROORMORE,
        
    [Sugar("+")]
    [LexemeLabel("fr","un ou plus")]
    [LexemeLabel("en","one or more")]
    [Mode("default","EXT")]
    ONEORMORE,
        
    [LexemeLabel("fr","optionnel")]
    [LexemeLabel("en","optional")]
    [Sugar("?")]
    OPTION ,
        
    [LexemeLabel("fr","écarter")]
    [LexemeLabel("en","discard")]
    [Sugar("[d]")]
    DISCARD,
        
    [LexemeLabel("fr","alternative")]
    [LexemeLabel("en","alternative")]
    [Sugar("|")]
    OR,
    
    [LexemeLabel("fr","mot clé 'Right'")]
    [LexemeLabel("en","'Right' keyword")]
    [Keyword("Right")] RIGHT,
    [LexemeLabel("fr","mot clé 'Left'")]
    [LexemeLabel("en","'Left' keyword")]
    [Keyword("Left")] LEFT,
    [LexemeLabel("fr","mot clé 'Operand'")]
    [LexemeLabel("en","'Operand' keyword")]
    [Keyword("Operand")] OPERAND,
    [LexemeLabel("fr","mot clé 'Prefix'")]
    [LexemeLabel("en","'Prefix' keyword")]
    [Keyword("Prefix")] PREFIX,
    [LexemeLabel("fr","mot clé 'Postfix'")]
    [LexemeLabel("en","'Postfix' keyword")]
    [Keyword("Postfix")] POSTFIX,
    
    [Push("EXT")]
    [Mode]
    [LexemeLabel("fr","ouverture de définition d'extension")]
    [LexemeLabel("en","extension definition opening")]
    [Sugar(">>>")]
    OPEN_EXT,
    
    [Mode("EXT","default")]
    [LexemeLabel("fr","flèche")]
    [LexemeLabel("en","arrow")]
    [Sugar("->")]
    ARROW,
    
    [LexemeLabel("fr","arobase")]
    [LexemeLabel("en","at")]
    [Mode("EXT","default")]
    [Sugar("@")]
    AT,
    
    [LexemeLabel("fr","tiret haut")]
    [LexemeLabel("en","dash")]
    [Mode("EXT")]
    [Sugar("-")]
    DASH,
    
    
    [Mode("EXT")]
    [LexemeLabel("fr","accolade ouvrante")]
    [LexemeLabel("en","opening curly brace")]
    [Sugar("{")]
    LEFTCURL,
    
    [LexemeLabel("fr","accolade fermante")]
    [LexemeLabel("en","closing curly brace")]
    [Mode("EXT")]
    [Sugar("}")]
    RIGHTCURL,
    
    [LexemeLabel("fr","virgule")]
    [LexemeLabel("en","comma")]
    [Mode("default","EXT")]
    [Sugar(",")]
    COMMA,
    
    [LexemeLabel("fr","mot clé END")]
    [LexemeLabel("en","END keyword")]
    [Mode("EXT")]
    [Keyword("END")]
    ENDTOKEN,
    
    
    [Pop]
    [Mode("EXT")]
    [LexemeLabel("fr","fermeture de définition d'extension")]
    [LexemeLabel("en","extension definition closing")]
    [Sugar("<<<")]
    CLOSE_EXT,
    
   
}