using System.Diagnostics.CodeAnalysis;
using csly.cli.model.parser;
using sly.lexer;

namespace csly.cli.model.lexer;

public class TokenModel : AttributedModel, ICLIModel
{
    
    public const string? LabelAttributeName = "label";   
    public GenericToken Type { get; set; }
    
    public IdentifierType IdentifierType { get; set; }
    
    public string Name { get; set; }
    
    public string[] Args { get; set; }
    
    

    public List<string> Modes { get; private set; } = new List<string>();
    
    public string PushMode { get; set; }
    
    public bool IsPop { get; set; }

    public bool IsPush => !string.IsNullOrEmpty(PushMode);


    public TokenModel(List<AttributeModel> attributes, GenericToken type, string name, IdentifierType identifierType = IdentifierType.Alpha)
    {
        Type = type;
        Name = name;
        IdentifierType = identifierType;
        SetAttributes(attributes);
    }
    public TokenModel(List<AttributeModel> attributes, GenericToken type, string name,  IdentifierType identifierType = IdentifierType.Alpha, params string[] args) : this(attributes, type,name,identifierType)
    {
        Args = args;
    }
    
    public TokenModel(List<AttributeModel> attributes, GenericToken type, string name,  params string[] args) : this(attributes, type,name,IdentifierType.Alpha)
    {
        Args = args;
    }

    protected TokenModel(GenericToken type, string name) : this(new List<AttributeModel>(), type, name)
    {
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

    public bool TryGetLabels(out Dictionary<string,string> labels)
    {
        if (TryGetValues(LabelAttributeName, out var values))
        {
            labels = values.ToDictionary(x => x[0], x => x[1]);
            return true;
        }
        labels = null;
        return false;
    }

    public LexerPosition Position { get; set; }
}