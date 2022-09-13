using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class OperandRule : Rule
    {
        public bool IsRule => false;
        public bool IsPrefix => false;
        public bool IsOperand => true;
        public bool IsInfix = false;
        public OperandRule()
        {
            Name = null;
        }

        public OperandRule(string name, bool isTerminal)
        {
            Name = name;
            IsToken = isTerminal;
        }

        public string Name { get; set;  }
        
        public bool IsToken { get; set;  }

       
       
    }
}