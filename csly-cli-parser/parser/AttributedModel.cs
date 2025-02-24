namespace csly.cli.model.parser;

public abstract class AttributedModel
{
    public Dictionary<string, List<AttributeModel>> Attributes = new Dictionary<string, List<AttributeModel>>();

    public void SetAttributes(IEnumerable<AttributeModel> attributes)
    {
        var grouped = attributes.GroupBy(x => x.AttributeName);
        Attributes = grouped.ToDictionary(x => x.Key, x => x.ToList());
    }

    protected bool TryGetFirstValue(string attributeName, out string value)
    {
        if (Attributes.TryGetValue(attributeName, out var attributes))
        {
            value = attributes[0].AttributeValue;
            return true;
        }

        value = null;
        return false;
    }

    protected bool TryGetValues(string attributeName, out List<List<string>> values)
    {
        if (Attributes.TryGetValue(attributeName, out var attributes))
        {
            values = attributes.Select(x => x.AttributeValues.ToList()).ToList();
            return true;
        }

        values = new List<List<string>>();
        return false;
    }
}