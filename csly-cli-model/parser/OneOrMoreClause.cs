using System.Diagnostics.CodeAnalysis;

namespace csly.cli.model.parser
{
    public class OneOrMoreClause : ManyClause
    {
        public OneOrMoreClause(IClause clause)
        {
            Clause = clause;
        }


        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Clause + "+";
        }

       
    }
}