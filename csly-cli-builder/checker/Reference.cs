namespace clsy.cli.builder.checker;

public class Reference
{
    public bool IsToken { get; set; }
    
    public bool IsRule { get; set; }
    
    public string Name { get; set; }

    public static Reference Rule(string name) => new Reference() { IsRule = true, IsToken = false, Name = name };
    
    public static Reference Token(string name) => new Reference() { IsRule = false, IsToken = true, Name = name };


}