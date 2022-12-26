using sly.lexer;
using sly.lexer.fsm;

namespace csly.cli.parser;

public class CLITokenExtensions
{
    public static void AddExtension(CLIToken token, LexemeAttribute lexem, GenericLexer<CLIToken> lexer)
    {
        if (token == CLIToken.CHAR)
        {
            NodeCallback<GenericToken> callback = match =>
            {
                match.Properties[GenericLexer<CLIToken>.DerivedToken] = CLIToken.CHAR;
                match.Result.Channel = Channels.Main;
                return match;
            };

            var fsmBuilder = lexer.FSMBuilder;

            fsmBuilder.GoTo(GenericLexer<CLIToken>.start)
                .ExceptTransition(new[]{'-',',','[',']'})
                .End(GenericToken.Extension)
                .CallBack(callback);
        }
    }
}