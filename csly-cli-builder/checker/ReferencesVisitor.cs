using clsy.cli.builder.parser.cli.model;
using csly.cli.model;
using csly.cli.model.lexer;
using csly.cli.model.parser;

namespace clsy.cli.builder.checker;

public class ReferencesVisitor : AbstractModelVisitor<RuleReferences>
{

    private string currentRule = null;

    public override RuleReferences Visit(ParserModel parser, RuleReferences result)
    {
        if (parser.Rules.Any(x => x.IsExpression))
        {
            result.AddRule($"{parser.Name}_expressions");
        }
        return result;
    }

    public override RuleReferences Visit(TokenModel token, RuleReferences result)
    {
        result.AddToken(token.Name);
        return result;
    }

    public override RuleReferences Visit(Rule rule, RuleReferences result)
    {
        currentRule = rule.RuleString;
        result.AddRule(rule.NonTerminalName);
        return result;
    }


    public override RuleReferences VisitManyClause(ManyClause many, RuleReferences result)
    {
        return result;
    }

    public override RuleReferences VisitNonTerminalClause(NonTerminalClause nonTerminalClause, RuleReferences result)
    {
        result.AddRuleReference(currentRule, nonTerminalClause.NonTerminalName);
        return result;
    }

    public override RuleReferences VisitTerminalClause(TerminalClause terminalClause, RuleReferences result)
    {
        result.AddTokenReference(currentRule, terminalClause.TokenName);
        return result;
    }



}