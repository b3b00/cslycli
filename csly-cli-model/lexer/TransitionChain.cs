
namespace csly.cli.model.lexer;

public class TransitionChain: ICLIModel
{
    
    public string StartingNodeName { get; set; }
    
    public bool IsEnded { get; set; }
    
    public IList<ITransition> Transitions { get; set; }

    public TransitionChain(IList<ITransition> transitions, bool isEnded = false)
    {
        Transitions = transitions;
        IsEnded = isEnded;
    }
    
}