using clsy.cli.builder.parser.cli.model;
using csly.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;

namespace clsy.cli.builder.checker;

public class ReferencesVisitor : AbstractModelVisitor<RuleReferences>
{

    private string currentRule = null;
    
    public RuleReferences Visit(TokenModel token, RuleReferences result)
    {
        result.AddToken(token.Name);
        return result;
    }

    public RuleReferences Visit(Rule rule, RuleReferences result)
    {
        currentRule = rule.RuleString;
        return result;
    }


    public RuleReferences VisitManyClause(ManyClause many, RuleReferences result)
    {
        return result;
    }

    public RuleReferences VisitNonTerminalClause(NonTerminalClause nonTerminalClause, RuleReferences result)
    {
        result.AddRuleReference(currentRule, nonTerminalClause.NonTerminalName);
        return result;
    }

    public RuleReferences VisitTerminalClause(TerminalClause terminalClause, RuleReferences result)
    {
        result.AddTokenReference(currentRule, terminalClause.TokenName);
        return result;
    }

}