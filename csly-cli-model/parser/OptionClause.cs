using System.Diagnostics.CodeAnalysis;

namespace csly.cli.model.parser
{
    public class OptionClause: IClause
    {
        public OptionClause(IClause clause)
        {
            Clause = clause;
        }

        public IClause Clause { get; set; }

        public override string ToString()
        {
            if (Clause is TerminalClause)
            {
                return Clause.ToString();
            }
            else
            {
                
                return $"{Clause.ToString()}?";
            }
        }
    }
}