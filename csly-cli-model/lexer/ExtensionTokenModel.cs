using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace csly.cli.model.lexer;

public class ExtensionTokenModel : TokenModel
{
    

    public IList<ITransition> Transitions { get; set; }



    public ExtensionTokenModel(string name, IList<ITransition> transitions) : base(GenericToken.Extension,name)
    {
        Transitions = transitions;
    }
    
    public ExtensionTokenModel( IList<ITransition> transitions) : this(string.Empty,transitions)
    {
        Transitions = transitions;
    }


    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"[{Type} {Name}] {string.Join(", ",Args)}";
    }
}