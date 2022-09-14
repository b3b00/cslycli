using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class PrefixRule : Rule
    {
        
        public override bool IsRule => false;
        public override bool IsPrefix => true;
        public override bool IsOperand => false;
        public override bool IsInfix => false;
        
        public PrefixRule()
        {
            Name = null;
        }

        public PrefixRule(string name,  int precedence)
        {
            Name = name;
            Precedence = precedence;
        }

        public string Name { get; set;  }
        public int Precedence { get; set; }

       
       
    }
}