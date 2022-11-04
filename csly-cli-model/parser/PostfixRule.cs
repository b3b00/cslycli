namespace csly.cli.model.parser;

public class PostfixRule : Rule
{
        
    public override bool IsRule => false;
    public override bool IsPostfix => true;
    public override bool IsOperand => false;
    public override bool IsInfix => false;
  
    public PostfixRule(string name, bool isExplicit,  int precedence)
    {
        Name = name;
        Precedence = precedence;
        IsExplicit = isExplicit;
    }
    
    public bool IsExplicit { get; set; } = false;
    public string Name { get; set;  }
    public int Precedence { get; set; }

       
       
}