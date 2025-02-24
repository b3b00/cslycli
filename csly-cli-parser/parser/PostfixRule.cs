using System.Text;

namespace csly.cli.model.parser;

public class PostfixRule : Rule
{
    private string _name;

    public override bool IsRule => false;
    public override bool IsPostfix => true;
    public override bool IsOperand => false;
    public override bool IsInfix => false;
    
    public bool IsExplicit { get; set; } = false;
    public string Name => _name;
    public int Precedence { get; set; }
  
    public PostfixRule(string name, bool isExplicit,  int precedence)
    {
        _name = name;
        if (isExplicit)
        {
            _name = _name.Substring(1, _name.Length - 2);
        }
        Precedence = precedence;
        IsExplicit = isExplicit;
    }
    
  

       
       
}

public class ManyPostfixRule : Rule
{
    private IList<PostfixRule> _postfixes;

    public override  bool IsRule => false;
    public override bool IsPrefix => false;
    public override bool IsOperand => false;
    public override bool IsInfix => true;

    public IList<PostfixRule> Postfixes => _postfixes;
     
    public ManyPostfixRule(IList<PostfixRule> rules)
    {
        _postfixes = rules;
    }
    
    public string GetName(ref int explicitPostfixCounter)
    {
        string name = ""; 
        if (TryGetMethodName(out name))
        {
            
        }
        else
        {
            StringBuilder nameBuilder = new StringBuilder();
            foreach (var postfix in Postfixes)
            {
                if (postfix.IsExplicit)
                {
                    nameBuilder.Append("postfix").Append(explicitPostfixCounter);
                    explicitPostfixCounter++;
                }
                else
                {
                    nameBuilder.Append(postfix.Name);
                }

                nameBuilder.Append("_");
            }
            name = nameBuilder.ToString();
        }

        return name;
    }
}