using sly.lexer;

namespace csly.cli.model.parser;

public class IndentClause : IClause
{
    public override string ToString()
    {
        return "INDENT";
    }

    public LexerPosition Position { get; set; }
}