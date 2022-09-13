using csly.cli.model;
using csly.cli.model.lexer;

namespace clsy.cli.builder.parser.cli.model;

public class LexerModel : ICLIModel
{
    public  List<TokenModel> Tokens { get; set; } 
    public LexerModel(List<TokenModel> tokens)
    {
        Tokens = tokens;
    }
}