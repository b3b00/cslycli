using sly.lexer;

namespace csly.cli.model.lexer;

public class ModeModel : ICLIModel
{
    public List<string> Modes { get; set; }

    public ModeModel(List<string> modes)
    {
        Modes = modes;
    } 
    
    public ModeModel(string mode, List<string> modes) : this(modes)
    {
        modes.Add(mode);
    }

    public LexerPosition Position { get; set; }
}