using System.Text.Json;
using clsy.cli.builder.parser.cli.model;
using csly.cli.model;
using LexerBuilder = clsy.cli.builder.lexer.LexerBuilder;
using sly.lexer;

namespace clsy.cli.builder.parser;

public class ParserBuilder
{

    public Type EnumType { get; set; }
    public Type TokenType { get; set; }
    
    
    public ParserBuilder()
    {
        
    }
    
    public (object parserBuildResult, Type parserType) BuildParser(ICLIModel model)
    {
        // TODO
        EnumType = LexerBuilder.BuildLexerEnum(model as LexerModel);
        TokenType = BuilderHelper.BuildGenericType(typeof(Token<>),EnumType);
        
        
        
        
        /*
         * 
         */
        return (null, null);
    }
}