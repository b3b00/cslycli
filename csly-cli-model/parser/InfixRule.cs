using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class InfixRule : Rule
    {
        
        public override  bool IsRule => false;
        public override bool IsPrefix => false;
        public override bool IsOperand => false;
        public override bool IsInfix => true;
        
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