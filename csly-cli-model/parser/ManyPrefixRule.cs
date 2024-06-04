using System.Text;

namespace csly.cli.model.parser;

public class ManyPrefixRule : Rule
{
    private IList<PrefixRule> _prefixes;

    public override  bool IsRule => false;
    public override bool IsPrefix => false;
    public override bool IsOperand => false;
    public override bool IsInfix => true;

    public IList<PrefixRule> Prefixes => _prefixes;
     
    public ManyPrefixRule(IList<PrefixRule> rules)
    {
        _prefixes = rules;
    }

    public string GetName(ref int explicitPrefixCounter)
    {
        string name = ""; 
        if (TryGetMethodName(out name))
        {
            
        }
        else
        {
            StringBuilder nameBuilder = new StringBuilder();
            foreach (var prefix in Prefixes)
            {
                if (prefix.IsExplicit)
                {
                    nameBuilder.Append("prefix").Append(explicitPrefixCounter);
                    explicitPrefixCounter++;
                }
                else
                {
                    nameBuilder.Append(prefix.Name);
                }

                nameBuilder.Append("_");
            }
            name = nameBuilder.ToString();
        }

        return name;
    }
}