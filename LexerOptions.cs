namespace csly.cli.model.parser;

public class LexerOptions : ICLIModel
{
    public bool IgnoreWS { get; set; }

    public bool IgnoreEOL { get; set; }

    public bool IgnoreKeyWordCase { get; set; }
    
    public bool IndentationAware { get; set; }
}