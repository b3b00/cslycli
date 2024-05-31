using csly.cli.model.parser;

namespace clsy.cli.builder.checker;

public static class ModelExtensions
{
    public static bool MayBeEmpty(this IClause clause)
    {
        return clause is ZeroOrMoreClause || clause is OptionClause;
    }

    public static IEnumerable<string> GetNonTerminals(this ParserModel model)
    {
        return model.Rules.Select(x => x.NonTerminalName).Distinct();
    }
    
    public static IEnumerable<Rule> GetRulesForNonTerminal(this ParserModel model, string nonTerminalName)
    {
        return model.Rules.Where(x => x.NonTerminalName == nonTerminalName);
    }
}