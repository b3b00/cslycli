using System.Diagnostics.CodeAnalysis;

namespace csly.cli.model.parser
{
    public class RepeatClause : ManyClause
    {
        
        public int Min { get; set; }
        
        public int Max { get; set; }

        public RepeatClause(IClause clause, int min) : this(clause, min, min)
        {
            
        }

        public RepeatClause(IClause clause, int min, int max)
        {
            
            Min = min;
            Max = max;
            Clause = clause;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Clause.ToString() + "*";
        }

      
    }
}