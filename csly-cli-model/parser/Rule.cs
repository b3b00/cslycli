using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sly.lexer;

namespace csly.cli.model.parser
{
    public class Rule :  AttributedModel, GrammarNode
    {

        public const string? methodNameAttribute = "name";        
        public Rule( bool  isOperand = false)
        {
            Clauses = new List<IClause>();
            _isOperand = isOperand;
        }

        public virtual bool IsRule => true;
        public virtual bool IsPrefix => false;
        
        public virtual bool IsPostfix => false;

        private bool _isOperand = false;
        public virtual bool IsOperand => _isOperand;
        public virtual bool IsInfix => false;

        public bool IsExpression => IsInfix || IsPrefix || IsPostfix;
        
        public string RuleString => NonTerminalName + " : " + string.Join(" ", Clauses.Select(x => x.ToString()));

        public string Key
        {
            get
            {
                // TODO ?? : rework Clause.ToString  
                var key = NonTerminalName+"_"+string.Join("_", Clauses.Select<IClause, string>(c => c.ToString()));
                
                if (Clauses.Count == 1) 
                    key += "_";

                return key;
            }
        }
        
        public List<IClause> Clauses { get; set; }

        public string NonTerminalName { get; set; }
        public bool IsRoot { get; set; }

        public bool TryGetMethodName(out string methodName)
        {
            methodName = null;
            return TryGetFirstValue(methodNameAttribute, out methodName);
        }

        public override string ToString()
        {
            return $"{NonTerminalName} : {string.Join(" ", Clauses.Select(x => x.ToString()))}";
        }

        public LexerPosition Position { get; set; }
    }
}