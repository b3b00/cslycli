using clsy.cli.model.lexer;
using csly.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;
using sly.lexer;

namespace clsy.cli.builder.parser.cli.model;

public class LexerModel : ICLIModel
{
    
    public LexerOptions Options { get; set; }
    public  List<TokenModel> Tokens { get; set; } 
    
    public Dictionary<string, List<TokenModel>> TokensByName { get; set; }

    public string Name { get; set; }

    public bool HasExtension => Tokens.Exists(x => x.Type == GenericToken.Extension);
    
    public LexerModel(List<TokenModel> tokens, LexerOptions options, string name)
    {
        Name = name;
        Tokens = tokens;
        Options = options;
        var grouped = tokens.GroupBy(x => x.Name);
        TokensByName = grouped.ToDictionary(x => x.Key, x => x.ToList());
    }

    public LexerPosition Position { get; set; }
}