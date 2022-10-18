using csly.cli.model;
using csly.cli.model.lexer;

namespace clsy.cli.builder.parser.cli.model;

public class LexerModel : ICLIModel
{
    public  List<TokenModel> Tokens { get; set; } 
    
    public string Name { get; set; }
    public LexerModel(List<TokenModel> tokens, string name)
    {
        Name = name;
        Tokens = tokens;
    }
}