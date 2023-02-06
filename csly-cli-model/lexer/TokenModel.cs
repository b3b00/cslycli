using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace csly.cli.model.lexer;

public class PushModel : ICLIModel
{
    public string Target { get; set; }

    public PushModel(string target)
    {
        Target = target;
    }
}

public class PopModel : ICLIModel
{
    public PopModel()
    {
    }
}

public class ModeModel : ICLIModel
{
    public List<string> Modes { get; set; }

    public ModeModel(List<string> modes)
    {
        Modes = modes;
    } 
    
    public ModeModel(string mode, List<string> modes) : this(modes)
    {
        modes.Add(mode);
    } 
}


public class TokenModel : ICLIModel
{
    public GenericToken Type { get; set; }
    
    public IdentifierType IdentifierType { get; set; }
    
    public string Name { get; set; }
    
    public string[] Args { get; set; }

    public List<string> Modes { get; private set; } = new List<string>();
    
    public string PushMode { get; set; }
    
    public bool IsPop { get; set; }

    public bool IsPush => !string.IsNullOrEmpty(PushMode);


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

    public void AddMode(string mode)
    {
        Modes.Add(mode);
    }
    
    public void AddModes(List<string> modes)
    {
        Modes.AddRange(modes);
    }
    
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return $"[{Type} {Name}] {string.Join(", ",Args)}";
    }
}