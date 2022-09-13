using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class Rule : GrammarNode
    {
        public Rule()
        {
            Clauses = new List<IClause>();
        }

        public Rule(string name, List<IClause> clauses)
        {
            NonTerminalName = name;
            Clauses = clauses;
        }

        public string RuleString { get; set;  }

        public string Key
        {
            get
            {
                var key = NonTerminalName+"_"+string.Join("_", Clauses.Select<IClause, string>(c => c.ToString()));
                
                if (Clauses.Count == 1) 
                    key += "_";

                return key;
            }
        }

        public List<IClause> Clauses { get; set; }

        public string NonTerminalName { get; set; }

       
    }
}