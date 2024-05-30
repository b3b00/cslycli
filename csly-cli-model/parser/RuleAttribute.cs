namespace csly.cli.model.parser;

public class RuleAttribute : ICLIModel
{
    private string _attributeName;
    private string _attributeValue;

    public string AttributeName => _attributeName;
        
    public string AttributeValue => _attributeValue;
    
    public RuleAttribute(string attributeName, string attributeValue)
    {
        _attributeName = attributeName;
        _attributeValue = attributeValue;
    }
}