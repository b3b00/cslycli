using sly.lexer;
using sly.lexer.fsm;
using sly.i18n;

namespace ns
{
    public enum someLexer
    {
        [LexemeLabel("en", "integer")]
        [LexemeLabel("fr", "entier")]
        [Mode("EXT", "default")]
        [Int]
        INT,
        [LexemeLabel("en", "plus sign")]
        [LexemeLabel("fr", "plus")]
        [Sugar("+")]
        PLUS,
        [LexemeLabel("en", "addition")]
        [LexemeLabel("fr", "addition")]
        [Keyword("add")]
        ADD,
        [LexemeLabel("en", "minus sign")]
        [LexemeLabel("fr", "moins")]
        [Sugar("-")]
        MINUS,
        [LexemeLabel("en", "substraction")]
        [LexemeLabel("fr", "soustraction")]
        [Keyword("remove")]
        REMOVE,
        [LexemeLabel("en", "times sign")]
        [LexemeLabel("fr", "fois")]
        [Sugar("*")]
        TIMES,
        [LexemeLabel("en", "multiplication")]
        [LexemeLabel("fr", "multiplication")]
        [Keyword("mul")]
        MUL,
        [LexemeLabel("en", "division sign")]
        [LexemeLabel("fr", "diviser")]
        [Sugar("/")]
        SLASH,
        [LexemeLabel("en", "division")]
        [LexemeLabel("fr", "division")]
        [Keyword("div")]
        DIV,
        [LexemeLabel("en", "test")]
        [LexemeLabel("fr", "test")]
        [Extension]
        TEST,
        [Push("EXT")]
        [Mode("EXT", "default")]
        [Sugar(">>>")]
        OPEN,
        [Pop]
        [Sugar("<<<")]
        CLOSE,
    }

    public class ExtendedsomeLexer
    {
        public static void ExtendsomeLexer(someLexer token, LexemeAttribute lexem, GenericLexer<someLexer> lexer)
        {
            if (token == someLexer.TEST)
            {
                NodeCallback<GenericToken> callbackTEST = (FSMMatch<GenericToken> match) =>
                {
                    // this store the token id the the FSMMatch object to be later returned by GenericLexer.Tokenize 
                    match.Properties[GenericLexer<someLexer>.DerivedToken] = someLexer.TEST;
                    return match;
                };
                var builder = lexer.FSMBuilder;
                builder.GoTo("start").Transition('#').MultiRangeTransition(('0', '9'), ('A', 'F')).MultiRangeTransition(('0', '9'), ('A', 'F')).MultiRangeTransition(('0', '9'), ('A', 'F')).MultiRangeTransition(('0', '9'), ('A', 'F')).MultiRangeTransition(('0', '9'), ('A', 'F')).MultiRangeTransition(('0', '9'), ('A', 'F')).End(GenericToken.Extension).CallBack(callbackTEST);
            }
        }
    }
}