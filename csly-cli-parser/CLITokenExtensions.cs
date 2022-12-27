using sly.lexer;
using sly.lexer.fsm;

namespace csly.cli.parser;

public static class CLITokenExtensions
{

    public static bool IsInteger(this string value)
    {
        return value.All(x => char.IsDigit(x));
    }
    
    public static Token<CLIToken> Map(Token<CLIToken> token)
    {
        var t = new Token<CLIToken>()
        {
            Position = token.Position,
            SpanValue = token.SpanValue,
            TokenID = token.TokenID,
            IsEOL = token.IsEOL,
            IsEOS = token.IsEOS,
            IsComment = token.IsComment,
            IsEmpty = token.IsEmpty,
            CommentType = token.CommentType,
            Channel = token.Channel,
            Discarded = token.Discarded
        };
        if (token.TokenID == CLIToken.CHAR && token.Value.Length > 1)
        {
            // TODO : this should not be needed
            if (token.Value == "END")
            {
                t.TokenID = CLIToken.END;
            }
            else if (token.Value.IsInteger())
            {
                t.TokenID = CLIToken.INT;
            }
            else
            {
                t.TokenID = CLIToken.ID;
            }
            
        }

        return t;
    } 
    
    public static List<Token<CLIToken>> LexerPostProcess(List<Token<CLIToken>> tokens)
    {
        List<Token<CLIToken>> newTokens = new List<Token<CLIToken>>();

        bool inMultiplier = false;
        
        for (int i = 0; i < tokens.Count; i++)
        {
            var current = tokens[i];
            
            if (inMultiplier)
            {
                if (current.TokenID == CLIToken.CHAR)
                {
                    newTokens.Add(new Token<CLIToken>()
                    {
                        Position =current.Position,
                        SpanValue = current.SpanValue,
                        TokenID = CLIToken.INT,
                        IsEOL = current.IsEOL,
                        IsEOS = current.IsEOS,
                        IsComment = current.IsComment,
                        IsEmpty = current.IsEmpty,
                        CommentType = current.CommentType,
                        Channel = current.Channel,
                        Discarded = current.Discarded
                    });
                }
                else
                {
                    newTokens.Add(Map(current));
                }
                inMultiplier = current.TokenID != CLIToken.RIGHTCURL;
            }
            else
            {
                newTokens.Add(Map(current));
                inMultiplier = current.TokenID == CLIToken.LEFTCURL; 
            }
        }

        return newTokens;
    }
    
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

            fsmBuilder.GoTo(GenericLexer<CLIToken>.in_identifier)
                .CallBack(callback);
            
            fsmBuilder.GoTo(GenericLexer<CLIToken>.in_int)
                .End(GenericToken.Extension)
                .CallBack(callback);
            
            fsmBuilder.GoTo(GenericLexer<CLIToken>.start)
                .ExceptTransition(new[]{'-',',','[',']','{','}'})
                .End(GenericToken.Extension)
                .CallBack(callback);
        }
    }
}