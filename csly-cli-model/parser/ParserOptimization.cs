using sly.lexer;
using sly.parser.generator;

namespace csly.cli.model.parser;

public class ParserOptimization : ICLIModel
{
    public bool UseMemoization { get; set; }
    public bool BroadenTokenWindow { get; set; }
    
    public bool AutoCloseIndentation { get; set; }

    public LexerPosition Position { get; set; }
}