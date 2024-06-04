using System.Text;
using sly.parser.generator;

namespace csly.cli.model.parser;

public class ManyInfixRule : Rule
{
    private IList<InfixRule> _infixes;

    public override  bool IsRule => false;
    public override bool IsPrefix => false;
    public override bool IsOperand => false;
    public override bool IsInfix => true;

    public IList<InfixRule> Infixes => _infixes;
     
    public ManyInfixRule(IList<InfixRule> rules)
    {
        _infixes = rules;
    }
    
    public string GetName(ref int explicitInfixCounter)
    {
        string name = ""; 
        if (TryGetMethodName(out name))
        {
            
        }
        else
        {
            StringBuilder nameBuilder = new StringBuilder();
            foreach (var infix in Infixes)
            {
                if (infix.IsExplicit)
                {
                    nameBuilder.Append("infix").Append(explicitInfixCounter);
                    explicitInfixCounter++;
                }
                else
                {
                    nameBuilder.Append(infix.Name);
                }

                nameBuilder.Append("_");
            }
            name = nameBuilder.ToString();
        }

        return name;
    }


       
       
}