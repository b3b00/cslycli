using sly.lexer;
using sly.lexer.fsm;

namespace ns
{
    public enum WhileLexer
    {
        [String("\"", "\\")]
        STRING,
        [Int]
        INT,
        [AlphaId]
        ID,
        [Keyword("if")]
        IF,
        [Keyword("then")]
        THEN,
        [Keyword("else")]
        ELSE,
        [Keyword("while")]
        WHILE,
        [Keyword("do")]
        DO,
        [Keyword("skip")]
        SKIP,
        [Keyword("true")]
        TRUE,
        [Keyword("false")]
        FALSE,
        [Keyword("not")]
        NOT,
        [Keyword("and")]
        AND,
        [Keyword("or")]
        OR,
        [Keyword("print")]
        PRINT,
        [Sugar(">")]
        GREATER,
        [Sugar("<")]
        LESSER,
        [Sugar("==")]
        EQUALS,
        [Sugar("!=")]
        DIFFERENT,
        [Sugar(".")]
        CONCAT,
        [Sugar(":=")]
        ASSIGN,
        [Sugar("+")]
        PLUS,
        [Sugar("-")]
        MINUS,
        [Sugar("*")]
        TIMES,
        [Sugar("/")]
        DIVIDE,
        [Sugar("(")]
        LPAREN,
        [Sugar(")")]
        RPAREN,
        [Sugar(";")]
        SEMICOLON,
    }
}