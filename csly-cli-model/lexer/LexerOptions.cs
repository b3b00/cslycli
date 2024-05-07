using csly.cli.model;

namespace clsy.cli.model.lexer;

public class LexerOptions : ICLIModel
{
    public bool? IgnoreWS { get; set; }

    public bool? IgnoreEOL { get; set; }

    public bool? IgnoreKeyWordCase { get; set; }
    
    public bool? IndentationAware { get; set; }
}