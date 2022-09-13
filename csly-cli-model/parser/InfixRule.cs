using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class InfixRule : Rule
    {
        public InfixRule()
        {
            Name = null;
        }

        public InfixRule(string name, Associativity assoc, int precedence)
        {
            Name = name;
            Associativity = assoc;
            Precedence = precedence;
        }

        public string Name { get; set;  }
        public Associativity  Associativity{ get; set;  }
        public int Precedence { get; set; }

       
       
    }
}