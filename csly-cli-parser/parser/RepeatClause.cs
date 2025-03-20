using System.Diagnostics.CodeAnalysis;

namespace csly.cli.model.parser
{
    public class RepeatClause : ManyClause
    {
        
        public int Min { get; set; }
        
        public int Max { get; set; }

        private bool _isRange => Min != Max;

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
            if (_isRange)
            {
                return $"{Clause} {{{Min}-{Max}}}";
            }
            
            return $"{Clause} {{{Min}}}";
        }

      
    }
}