using sly.lexer;

namespace csly.cli.model.lexer;

public interface ITransition : ICLIModel
{
    TransitionRepeater Repeater { get; set; }
    
    string Mark { get; set; }
    
    string Target { get; set; }
    
}

public enum RepeaterType
{
    Option,
    OneOrMore,
    ZeroOrMore,
    Count
}

public class TransitionRepeater : ICLIModel {
    public TransitionRepeater(RepeaterType repeaterType)
    {
        RepeaterType = repeaterType;
    }
    
    public TransitionRepeater(RepeaterType repeaterType, int count) : this(repeaterType)
    {
        Count = count;
    }

    public RepeaterType RepeaterType { get; private set; }
    
    public int Count { get; private set; }

    public LexerPosition Position { get; set; }
}