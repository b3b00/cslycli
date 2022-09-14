using System.Diagnostics.CodeAnalysis;

namespace csly.cli.model.parser
{
    public class NonTerminalClause: IClause
    {
        public NonTerminalClause(string name)
        {
            NonTerminalName = name;
        }

        public string NonTerminalName { get; set; }

        public override string ToString()
        {
            return NonTerminalName;
        }
    }
}