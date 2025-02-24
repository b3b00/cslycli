using sly.lexer;

namespace csly.cli.model;

public interface ICLIModel
{
    LexerPosition Position { get; set; }
    
}