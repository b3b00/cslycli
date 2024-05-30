
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class InfixRule : Rule
    {
        
        public override  bool IsRule => false;
        public override bool IsPrefix => false;
        public override bool IsOperand => false;
        public override bool IsInfix => true;
     
        public InfixRule(string name, bool isExplicit, Associativity assoc, int precedence)
        {
            Name = name;
            Associativity = assoc;
            Precedence = precedence;
            IsExplicit = isExplicit;
        }

        public string Name { get; set;  }
        
        public bool IsExplicit { get; set; } = false;
        public Associativity  Associativity{ get; set;  }
        public int Precedence { get; set; }

       
       
    }
}