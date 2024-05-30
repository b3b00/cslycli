using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace ns
{
    [ParserRoot("statement")]
    public class WhileParser
    {
        [Infix("LESSER", Associativity.Right, 50)]
        public int LESSER(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("GREATER", Associativity.Right, 50)]
        public int GREATER(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("EQUALS", Associativity.Right, 50)]
        public int EQUALS(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("DIFFERENT", Associativity.Right, 50)]
        public int DIFFERENT(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("CONCAT", Associativity.Right, 10)]
        public int CONCAT(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("PLUS", Associativity.Right, 10)]
        public int PLUS(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("MINUS", Associativity.Left, 10)]
        public int MINUS(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("TIMES", Associativity.Right, 50)]
        public int TIMES(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("DIVIDE", Associativity.Left, 50)]
        public int DIVIDE(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Prefix("MINUS", Associativity.Left, 100)]
        public int MINUS(Token<WhileLexer> oper, int value)
        {
            return value;
        }

        [Infix("OR", Associativity.Right, 10)]
        public int OR(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Infix("AND", Associativity.Right, 50)]
        public int AND(int left, Token<WhileLexer> oper, int right)
        {
            return left;
        }

        [Prefix("NOT", Associativity.Left, 100)]
        public int NOT(Token<WhileLexer> oper, int value)
        {
            return value;
        }

        [Production("statement : LPAREN statement RPAREN")]
        public int program(Token<WhileLexer> p0, int p1, Token<WhileLexer> p2)
        {
            return default(int);
        }

        [Production("statement : sequence")]
        public int sequence(int p0)
        {
            return default(int);
        }

        [Operand]
        [Production("operand : [ INT | TRUE | FALSE | STRING | ID ]")]
        public int operand_____INT___TRUE___FALSE___STRING___ID__(Token<WhileLexer> p0)
        {
            return default(int);
        }

        [Operand]
        [Production("operand : LPAREN WhileParser_expressions RPAREN")]
        public int operand___LPAREN_WhileParser_expressions_RPAREN(Token<WhileLexer> p0, int p1, Token<WhileLexer> p2)
        {
            return default(int);
        }

        [Production("sequence : statementPrim additionalStatements *")]
        public int sequenceStatement(int p0, List<int> p1)
        {
            return default(int);
        }

        [Production("additionalStatements : SEMICOLON statementPrim")]
        public int additionalStatement(Token<WhileLexer> p0, int p1)
        {
            return default(int);
        }

        [Production("statementPrim : IF WhileParser_expressions THEN statement ELSE statement")]
        public int conditionalStatement(Token<WhileLexer> p0, int p1, Token<WhileLexer> p2, int p3, Token<WhileLexer> p4, int p5)
        {
            return default(int);
        }

        [Production("statementPrim : WHILE WhileParser_expressions DO statement")]
        public int whileStatement(Token<WhileLexer> p0, int p1, Token<WhileLexer> p2, int p3)
        {
            return default(int);
        }

        [Production("statementPrim : ID ASSIGN WhileParser_expressions")]
        public int assignmentStatement(Token<WhileLexer> p0, Token<WhileLexer> p1, int p2)
        {
            return default(int);
        }

        [Production("statementPrim : SKIP")]
        public int skipStatement(Token<WhileLexer> p0)
        {
            return default(int);
        }

        [Production("statementPrim : PRINT WhileParser_expressions")]
        public int printStatement(Token<WhileLexer> p0, int p1)
        {
            return default(int);
        }
    }
}