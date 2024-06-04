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
}