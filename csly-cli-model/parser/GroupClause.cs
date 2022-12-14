using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;

namespace csly.cli.model.parser
{
    [DebuggerDisplay("{ToString()}")]
    public class GroupClause : IClause
    {
        public GroupClause(IClause clause)
        {
            Clauses = new List<IClause> {clause};
        }
     
        public GroupClause(ChoiceClause choices)
        {
            Clauses = new List<IClause> {choices};
        }

        public List<IClause> Clauses { get; set; }

        [ExcludeFromCodeCoverage]
        public bool MayBeEmpty()
        {
            return true;
        }

        public void AddRange(GroupClause clauses)
        {
            Clauses.AddRange(clauses.Clauses);
        }
        
        public override string ToString()
        {
            return $"({string.Join(" ",Clauses.Select(x => x.ToString()))})";
        }
        
       
    }
}