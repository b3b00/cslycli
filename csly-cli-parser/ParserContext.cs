namespace csly.cli.parser;

public class ParserContext
{
    private List<string> EnumNames = new List<string>();

    public string _parserName;

    public List<string> Errors { get; set; }= new List<string>();

    public bool IsError { get; set; } = false;
    
    public void AddError(string error)
    {
        IsError = true;
        Errors.Add(error);
    }

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