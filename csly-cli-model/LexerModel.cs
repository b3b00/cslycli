namespace csly_cli_model;

public class LexerModel : ICLIModel
{
    public  List<TokenModel> Tokens { get; set; } 
    public LexerModel(List<TokenModel> tokens)
    {
        Tokens = tokens;
    }
}