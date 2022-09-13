namespace csly.cli.parser;

public class ParserContext
{
    private List<string> EnumNames = new List<string>();

    public void AddEnumName(string name)
    {
        EnumNames.Add(name);
    }

    public bool IsTerminal(string name) => EnumNames.Contains(name);
}