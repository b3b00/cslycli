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


       
       
}