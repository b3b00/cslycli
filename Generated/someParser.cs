using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace ns
{
    [ParserRoot("root")]
    public class someParser
    {
        [Production("root : someParser_expressions")]
        public int root_someParserexpressions(int p0)
        {
            return default(int);
        }

        [Infix("PLUS", Associativity.Right, 10)]
        [Infix("ADD", Associativity.Right, 10)]
        public int PLUS_ADD_(int left, Token<someLexer> oper, int right)
        {
            return left;
        }

        [Infix("MINUS", Associativity.Right, 10)]
        [Infix("REMOVE", Associativity.Right, 10)]
        public int minus(int left, Token<someLexer> oper, int right)
        {
            return left;
        }

        [Infix("TIMES", Associativity.Right, 50)]
        [Infix("MUL", Associativity.Right, 50)]
        public int TIMES_MUL_(int left, Token<someLexer> oper, int right)
        {
            return left;
        }

        [Infix("DIV", Associativity.Right, 50)]
        [Infix("SLASH", Associativity.Right, 50)]
        public int div(int left, Token<someLexer> oper, int right)
        {
            return left;
        }

        [Prefix("PLUS", Associativity.Left, 100)]
        [Prefix("ADD", Associativity.Left, 100)]
        [Prefix("'#'", Associativity.Left, 100)]
        public int PLUS_ADD_prefix0_(Token<someLexer> oper, int value)
        {
            return value;
        }

        [Prefix("MINUS", Associativity.Left, 100)]
        [Prefix("REMOVE", Associativity.Left, 100)]
        [Prefix("'~'", Associativity.Left, 100)]
        public int MINUS_REMOVE_prefix1_(Token<someLexer> oper, int value)
        {
            return value;
        }

        [Operand]
        [Production("integer : INT")]
        public int integer_INT(Token<someLexer> p0)
        {
            return default(int);
        }
    }
}