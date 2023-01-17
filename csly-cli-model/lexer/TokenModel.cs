using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace csly.cli.model.lexer;

public class TokenModel : ICLIModel
{
    public GenericToken Type { get; set; }
    
    public IdentifierType IdentifierType { get; set; }
    
    public string Name { get; set; }
    
    public string[] Args { get; set; }


    public TokenModel(GenericToken type, string name, IdentifierType identifierType = IdentifierType.Alpha)
    {
        Type = type;
        Name = name;
        IdentifierType = identifierType;
    }
    public TokenModel(GenericToken type, string name,  IdentifierType identifierType = IdentifierType.Alpha, params string[] args) : this(type,name,identifierType)
    {
        Args = args;
    }
    
    public TokenModel(GenericToken type, string name,  params string[] args) : this(type,name,IdentifierType.Alpha)
    {
        Args = args;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"[{Type} {Name}] {string.Join(", ",Args)}";
    }
}