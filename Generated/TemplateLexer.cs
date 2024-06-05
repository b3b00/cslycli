using sly.lexer;
using sly.lexer.fsm;
using sly.i18n;

namespace ns
{
    public enum TemplateLexer
    {
        [Sugar(" ")]
        SP,
        [Sugar("	")]
        TAB,
        [Sugar(" ")] CRLF, [Sugar(" ")] LF, [UpTo("{%", "{=")] TEXT, [Push("code")][Sugar("{%")] OPEN_CODE, [Push("value")][Sugar("{=")] OPEN_VALUE, [Mode("value", "code")][AlphaId] ID, [Pop][Mode("value")][Sugar("=}")] CLOSE_VALUE, [Pop][Mode("code")][Sugar("%}")] CLOSE_CODE, [Mode("code")][Keyword("if")] IF, [Mode("code")][Keyword("endif")] ENDIF, [Mode("code")][Keyword("else")] ELSE, [Mode("code")][Keyword("for")] FOR, [Mode("code")][Keyword("as")] AS, [Mode("code")][Keyword("end")] END, [Mode("code")][Sugar("..")] RANGE, [Mode("code")][String("\"", "\\")] STRING, [Mode("code")][Int] INT, [Mode("code")][Keyword("TRUE")][Keyword("true")] TRUE, [Mode("code")][Keyword("FALSE")][Keyword("false")] FALSE, [Mode("code")][Sugar(">")] GREATER, [Mode("code")][Sugar("<")] LESSER, [Mode("code")][Sugar("==")] EQUALS, [Mode("code")][Sugar("!=")] DIFFERENT, [Mode("code")][Sugar("&")] CONCAT, [Mode("code")][Sugar(":=")] ASSIGN, [Mode("code")][Sugar("+")] PLUS, [Mode("code")][Sugar("-")] MINUS, [Mode("code")][Sugar("*")] TIMES, [Mode("code")][Sugar("/")] DIVIDE, [Mode("code")][Sugar("(")] OPEN_PAREN, [Mode("code")][Sugar(")")] CLOSE_PAREN, [Mode("code")][Keyword("NOT")][Keyword("not")] NOT, [Mode("code")][Keyword("AND")][Keyword("and")] AND, [Mode("code")][Keyword("OR")][Keyword("or")] OR, }
}