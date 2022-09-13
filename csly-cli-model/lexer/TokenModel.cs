using sly.lexer;

namespace csly.cli.model.lexer;

public class TokenModel : ICLIModel
{
    public GenericToken Type { get; set; }
    
    public string Name { get; set; }
    
    public string[] Args { get; set; }

    

    public TokenModel(GenericToken type, string name)
    {
        Type = type;
        Name = name;
        Args = new [] {""};
    }
    public TokenModel(GenericToken type, string name, params string[] args) : this(type,name)
    {
        Args = args;
    }

    public override string ToString()
    {
        return $"[{Type} {Name}] {string.Join(", ",Args)}";
    }
}