namespace csly.cli.parser;

public class ParserContext
{
    private List<string> EnumNames = new List<string>();

    public string _parserName;

    public ParserContext(string parserName)
    {
        _parserName = parserName;
    }

    public void AddEnumName(string name)
    {
        EnumNames.Add(name);
    }

    public bool IsTerminal(string name) => EnumNames.Contains(name);
}