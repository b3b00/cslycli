using sly.lexer;

namespace csly.cli.model.lexer;

public class RangeTransition : ITransition
{
    
    public RangeTransition(IList<Range> ranges)
    {
        Ranges = ranges;
    }
    
    
    public RangeTransition(TransitionRepeater repeater, IList<Range> ranges) : this(ranges)
    {
        Repeater = repeater;
    }

    public TransitionRepeater Repeater { get; set; }
    
    public IList<Range> Ranges { get; set; }
    
    public string Mark { get; set; }
    
    public string Target { get; set; }
    public LexerPosition Position { get; set; }
}