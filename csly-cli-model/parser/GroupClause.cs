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

        public GroupClause()
        {
            Clauses = new List<IClause>();
        }

        public GroupClause(IClause clause) : this()
        {
            Clauses.Add(clause);
        }
     
        public GroupClause(ChoiceClause choices)
        {
            Clauses = new List<IClause> {choices};
        }
        
        public GroupClause(List<IClause> choices)
        {
            Clauses = choices;
        }

        public List<IClause> Clauses { get; set; }

        [ExcludeFromCodeCoverage]
        public bool MayBeEmpty()
        {
            return true;
        }

        public void AddRange(List<IClause> clauses)
        {
            Clauses.AddRange(clauses);
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