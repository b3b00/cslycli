
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class InfixRule : Rule
    {
        private string _name;
        
        public override  bool IsRule => false;
        public override bool IsPrefix => false;
        public override bool IsOperand => false;
        public override bool IsInfix => true;
     
        public InfixRule(string name, bool isExplicit, Associativity assoc, int precedence)
        {
            _name = name;
            if (isExplicit)
            {
                _name = _name.Substring(1, _name.Length - 2);
            }

            Associativity = assoc;
            Precedence = precedence;
            IsExplicit = isExplicit;
        }

        public string Name => _name;
        
        public bool IsExplicit { get; set; } = false;
        public Associativity  Associativity{ get; set;  }
        public int Precedence { get; set; }

       
       
    }
}