using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace ns
{
    [ParserRoot("template")]
    public class TemplateParser
    {
        [Production("template : item *")]
        public object template_item_(List<object> p0)
        {
            return default(object);
        }

        [Production("item : TEXT")]
        public object item_TEXT(Token<TemplateLexer> p0)
        {
            return default(object);
        }

        [Production("item : OPEN_VALUE ID CLOSE_VALUE")]
        public object item_OPENVALUE_ID_CLOSEVALUE(Token<TemplateLexer> p0, Token<TemplateLexer> p1, Token<TemplateLexer> p2)
        {
            return default(object);
        }

        [Production("item : OPEN_CODE IF OPEN_PAREN TemplateParser_expressions CLOSE_PAREN CLOSE_CODE item * elseBlock? OPEN_CODE ENDIF CLOSE_CODE")]
        public object item_OPENCODE_IF_OPENPAREN_TemplateParserexpressions_CLOSEPAREN_CLOSECODE_item_elseBlock_OPENCODE_ENDIF_CLOSECODE(Token<TemplateLexer> p0, Token<TemplateLexer> p1, Token<TemplateLexer> p2, object p3, Token<TemplateLexer> p4, Token<TemplateLexer> p5, List<object> p6, ValueOption<object> p7, Token<TemplateLexer> p8, Token<TemplateLexer> p9, Token<TemplateLexer> p10)
        {
            return default(object);
        }

        [Production("if : OPEN_CODE IF OPEN_PAREN TemplateParser_expressions CLOSE_PAREN CLOSE_CODE")]
        public object if_OPENCODE_IF_OPENPAREN_TemplateParserexpressions_CLOSEPAREN_CLOSECODE(Token<TemplateLexer> p0, Token<TemplateLexer> p1, Token<TemplateLexer> p2, object p3, Token<TemplateLexer> p4, Token<TemplateLexer> p5)
        {
            return default(object);
        }

        [Production("elseBlock : OPEN_CODE ELSE CLOSE_CODE item *")]
        public object elseBlock_OPENCODE_ELSE_CLOSECODE_item_(Token<TemplateLexer> p0, Token<TemplateLexer> p1, Token<TemplateLexer> p2, List<object> p3)
        {
            return default(object);
        }

        [Production("item : OPEN_CODE FOR INT RANGE INT AS ID CLOSE_CODE item * OPEN_CODE END CLOSE_CODE")]
        public object item_OPENCODE_FOR_INT_RANGE_INT_AS_ID_CLOSECODE_item_OPENCODE_END_CLOSECODE(Token<TemplateLexer> p0, Token<TemplateLexer> p1, Token<TemplateLexer> p2, Token<TemplateLexer> p3, Token<TemplateLexer> p4, Token<TemplateLexer> p5, Token<TemplateLexer> p6, Token<TemplateLexer> p7, List<object> p8, Token<TemplateLexer> p9, Token<TemplateLexer> p10, Token<TemplateLexer> p11)
        {
            return default(object);
        }

        [Production("item : OPEN_CODE FOR ID AS ID CLOSE_CODE item * OPEN_CODE END CLOSE_CODE")]
        public object item_OPENCODE_FOR_ID_AS_ID_CLOSECODE_item_OPENCODE_END_CLOSECODE(Token<TemplateLexer> p0, Token<TemplateLexer> p1, Token<TemplateLexer> p2, Token<TemplateLexer> p3, Token<TemplateLexer> p4, Token<TemplateLexer> p5, List<object> p6, Token<TemplateLexer> p7, Token<TemplateLexer> p8, Token<TemplateLexer> p9)
        {
            return default(object);
        }

        [Infix("LESSER", Associativity.Right, 50)]
        public object LESSER(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Infix("GREATER", Associativity.Right, 50)]
        public object GREATER(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Infix("EQUALS", Associativity.Right, 50)]
        public object EQUALS(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Infix("DIFFERENT", Associativity.Right, 50)]
        public object DIFFERENT(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Production("primary : INT")]
        public object primary_INT(Token<TemplateLexer> p0)
        {
            return default(object);
        }

        [Production("primary : TRUE")]
        public object primary_TRUE(Token<TemplateLexer> p0)
        {
            return default(object);
        }

        [Production("primary : FALSE")]
        public object primary_FALSE(Token<TemplateLexer> p0)
        {
            return default(object);
        }

        [Production("primary : STRING")]
        public object primary_STRING(Token<TemplateLexer> p0)
        {
            return default(object);
        }

        [Production("primary : ID")]
        public object primary_ID(Token<TemplateLexer> p0)
        {
            return default(object);
        }

        [Operand]
        [Production("operand : primary")]
        public object operand_primary(object p0)
        {
            return default(object);
        }

        [Infix("PLUS", Associativity.Right, 10)]
        public object PLUS(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Infix("MINUS", Associativity.Right, 10)]
        public object MINUS(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Infix("TIMES", Associativity.Right, 50)]
        public object TIMES(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Infix("DIVIDE", Associativity.Right, 50)]
        public object DIVIDE(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Prefix("MINUS", Associativity.Left, 100)]
        public object MINUS(Token<TemplateLexer> oper, object value)
        {
            return value;
        }

        [Infix("OR", Associativity.Right, 10)]
        public object OR(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Infix("AND", Associativity.Right, 50)]
        public object AND(object left, Token<TemplateLexer> oper, object right)
        {
            return left;
        }

        [Prefix("NOT", Associativity.Left, 100)]
        public object NOT(Token<TemplateLexer> oper, object value)
        {
            return value;
        }
    }
}