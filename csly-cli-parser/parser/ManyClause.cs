using sly.lexer;

namespace csly.cli.model.parser
{
    public abstract class ManyClause: IClause
    {
        public IClause Clause { get; set; }

        public LexerPosition Position { get; set; }
    }
}