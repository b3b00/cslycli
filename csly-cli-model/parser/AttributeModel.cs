using sly.lexer;

namespace csly.cli.model.parser;

public class AttributeModel : ICLIModel
{
    private string _attributeName;
    private IList<string> _attributeValues;

    public string AttributeName => _attributeName;

    public string AttributeValue => _attributeValues != null && _attributeValues.Any() ? AttributeValues[0] : null;
    
    public IList<string> AttributeValues => _attributeValues;
    
    public AttributeModel(string attributeName, IEnumerable<string> attributeValues)
    {
        _attributeName = attributeName;
        _attributeValues = attributeValues.ToList();
    }

    public LexerPosition Position { get; set; }
}