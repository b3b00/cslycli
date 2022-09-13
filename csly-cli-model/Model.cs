using clsy.cli.builder.parser.cli.model;
using csly.cli.model.parser;

namespace csly.cli.model;

public class Model : ICLIModel
{
    public Model(LexerModel lexer, ParserModel parser)
    {
        LexerModel = lexer;
        ParserModel = parser;
    }
    
    public LexerModel LexerModel { get; set; }
    
    public ParserModel ParserModel { get; set; }
}