using sly.lexer;

namespace csly.cli.model.lexer;

public class ExceptTransition : ITransition
{
    public TransitionRepeater Repeater { get; set; }
    
    public string Mark { get; set; }
    
    public string Target { get; set; }
    public LexerPosition Position { get; set; }
}