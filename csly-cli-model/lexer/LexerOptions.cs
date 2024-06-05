using csly.cli.model;
using sly.lexer;

namespace clsy.cli.model.lexer;

public class LexerOptions : ICLIModel
{
    public bool? IgnoreWS { get; set; }

    public bool? IgnoreEOL { get; set; }

    public bool? IgnoreKeyWordCase { get; set; }
    
    public bool? IndentationAware { get; set; }
    public LexerPosition Position { get; set; }

    public bool HasOptions => IgnoreWS.HasValue || IgnoreEOL.HasValue || IgnoreKeyWordCase.HasValue || IndentationAware.HasValue;
    
}