using System.Diagnostics.CodeAnalysis;

namespace csly.cli.model.parser
{
    public class ZeroOrMoreClause : ManyClause
    {
        
        public ZeroOrMoreClause(IClause clause)
        {
            Clause = clause;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Clause + "*";
        }

      
    }
}