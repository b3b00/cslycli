namespace csly.cli.model.lexer;

public class CharacterTransition : ITransition
{
    public CharacterTransition(char character)
    {
        Character = character;
    }

    public TransitionRepeater Repeater { get; set; }
    
    public char Character { get; set; }
    
    public string Mark { get; set; }
    
    public string Target { get; set; }
}