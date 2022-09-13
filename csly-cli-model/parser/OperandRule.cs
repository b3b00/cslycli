using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class OperandRule : Rule
    {
        public OperandRule()
        {
            Name = null;
        }

        public OperandRule(string name)
        {
            Name = name;
        }

        public string Name { get; set;  }

       
       
    }
}