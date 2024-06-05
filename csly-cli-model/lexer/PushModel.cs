using sly.lexer;

namespace csly.cli.model.lexer;

public class PushModel : ICLIModel
{
    public string Target { get; set; }

    public PushModel(string target)
    {
        Target = target;
    }

    public LexerPosition Position { get; set; }
}