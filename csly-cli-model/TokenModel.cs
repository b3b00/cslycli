using sly.lexer;

namespace csly_cli_model;

public class TokenModel : ICLIModel
{
    public GenericToken Type { get; set; }
    
    public string Name { get; set; }
    
    public string Arg { get; set; }

    

    public TokenModel(GenericToken type, string name)
    {
        Type = type;
        Name = name;
        Arg = "";
    }
    public TokenModel(GenericToken type, string name, string arg) : this(type,name)
    {
        Arg = arg;
    }

    public override string ToString()
    {
        return $"[{Type} {Name}] {Arg}";
    }
}