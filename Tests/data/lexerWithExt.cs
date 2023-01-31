
using sly.lexer;
using sly.lexer.fsm;

namespace ns {

    public enum ExtensionLexer {


		[Extension]
		TEST,

		[Extension]
		AT,



    }


    public class ExtendedExtensionLexer {

        public static void ExtendExtensionLexer(ExtensionLexer token, LexemeAttribute lexem, GenericLexer<ExtensionLexer> lexer) {
            
if (token == ExtensionLexer.TEST) {

NodeCallback<GenericToken> callbackTEST = (FSMMatch<GenericToken> match) => 
   	{
        // this store the token id the the FSMMatch object to be later returned by GenericLexer.Tokenize 
        match.Properties[GenericLexer<ExtensionLexer>.DerivedToken] = ExtensionLexer.TEST;
        return match;
            };

var builder = lexer.FSMBuilder;

builder.GoTo("start")
.Transition('#')

.MultiRangeTransition(('0', '9'), ('A', 'F'))
.MultiRangeTransition(('0', '9'), ('A', 'F'))
.MultiRangeTransition(('0', '9'), ('A', 'F'))
.MultiRangeTransition(('0', '9'), ('A', 'F'))
.MultiRangeTransition(('0', '9'), ('A', 'F'))
.MultiRangeTransition(('0', '9'), ('A', 'F'))

.CallBack(callbackTEST);

}if (token == ExtensionLexer.AT) {

NodeCallback<GenericToken> callbackAT = (FSMMatch<GenericToken> match) => 
   	{
        // this store the token id the the FSMMatch object to be later returned by GenericLexer.Tokenize 
        match.Properties[GenericLexer<ExtensionLexer>.DerivedToken] = ExtensionLexer.AT;
        return match;
            };

var builder = lexer.FSMBuilder;

builder.GoTo("start")
.Transition('@')

.CallBack(callbackAT);

}

        }    
    }

}