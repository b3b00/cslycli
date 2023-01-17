namespace csly.cli.model.lexer;

public interface ITransition : ICLIModel
{
    public TransitionRepeater Repeater { get; set; }
    
}

public class CharacterTransition : ITransition
{
    public CharacterTransition(char character)
    {
        Character = character;
    }

    public TransitionRepeater Repeater { get; set; }
    
    public char Character { get; set; }
}

public class Range : ICLIModel
{

    public char StartCharacter { get; private set; }
    
    public char EndCharacter { get; private set; }
    
    public Range(char startCharacter, char endCharacter)
    {
        StartCharacter = startCharacter;
        EndCharacter = endCharacter;
    }
}

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
} 

public class ExceptTransition : ITransition
{
    public TransitionRepeater Repeater { get; set; }
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
    
}