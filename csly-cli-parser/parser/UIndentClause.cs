using sly.lexer;

namespace csly.cli.model.parser;

public class UIndentClause : IClause
{
    public override string ToString()
    {
        return "UINDENT";
    }

    public LexerPosition Position { get; set; }
}