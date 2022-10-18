using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class OperandRule : Rule
    {
        public override bool IsRule => false;
        public override bool IsPrefix => false;
        public override bool IsOperand => true;
        public override bool IsInfix => false;
       
        public OperandRule(string name, bool isTerminal)
        {
            Name = name;
            IsToken = isTerminal;
        }

        public string Name { get; set;  }
        
        public bool IsToken { get; set;  }

       
       
    }
}