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
        Counts = new List<int>();
        Count = count;
    }
    
    public TransitionRepeater(RepeaterType repeaterType, IList<int> counts) : this(repeaterType)
    {
        Counts = counts;
    }

    public RepeaterType RepeaterType { get; private set; }

    public int Count
    {
        get
        {
            if (Counts != null && Counts.Any())
            {
                return Counts[0];
            }

            return 0;
        }
        private set
        {
            if (Counts == null)
            {
                Counts = new List<int>();
            }
            Counts[0] = value;
        }
    }

    public IList<int> Counts { get; set; }
    
}