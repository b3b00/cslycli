using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace csly.cli.model.lexer;

public class ExtensionTokenModel : TokenModel
{
    

    public IList<TransitionChain> Chains { get; set; }



    public ExtensionTokenModel(string name, IList<TransitionChain> chains) : base(GenericToken.Extension,name)
    {
        Chains = chains;
    }
    
    public ExtensionTokenModel( IList<TransitionChain> chains) : this(string.Empty,chains)
    {
        Chains = chains;
    }


    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"[{Type} {Name}] {string.Join(", ",Args)}";
    }
}