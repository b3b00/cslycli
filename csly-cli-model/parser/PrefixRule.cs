using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.parser.generator;

namespace csly.cli.model.parser
{
    public class PrefixRule : Rule
    {
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