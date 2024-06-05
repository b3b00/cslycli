using sly.lexer;

namespace csly.cli.model.lexer;

public class PopModel : ICLIModel
{
    public PopModel()
    {
    }

    public LexerPosition Position { get; set; }
}