namespace csly.cli.model.lexer;

public class Range : ICLIModel
{

    public char StartCharacter { get; private set; }
    
    public char EndCharacter { get; private set; }
    
    public Range(char startCharacter, char endCharacter)
    {
        StartCharacter = startCharacter;
        EndCharacter = endCharacter;
    }
}