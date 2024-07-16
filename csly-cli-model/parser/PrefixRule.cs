using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace csly.cli.model.parser
{
    public class PrefixRule : Rule
    {
        private string _name;
        
        public override bool IsRule => false;
        public override bool IsPrefix => true;
        public override bool IsOperand => false;
        public override bool IsInfix => false;
        
        public string Name => _name;
        public bool IsExplicit { get; set; } = false;
  
        public PrefixRule(string name, bool isExplicit,  int precedence)
        {
            _name = name;
            if (isExplicit)
            {
                _name = _name.Substring(1, _name.Length - 2);
            }
            Precedence = precedence;
            IsExplicit = isExplicit;
        }

        public int Precedence { get; set; }

       
       
    }
}