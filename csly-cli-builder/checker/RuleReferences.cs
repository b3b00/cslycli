namespace clsy.cli.builder.checker;

public class RuleReferences
{
    public Dictionary<string, IList<Reference>> References = new Dictionary<string, IList<Reference>>();

    public HashSet<string> tokenNames = new HashSet<string>();

    public HashSet<string> ruleNames = new HashSet<string>();


    public void AddRule(string rule)
    {
        ruleNames.Add(rule);
    }
    
    public void AddToken(string token)
    {
        ruleNames.Add(token);
    }
    
    private void AddReference(string ruleName, Reference reference)
    {
        IList<Reference> references = new List<Reference>();
        if (References.ContainsKey(ruleName))
        {
            references = References[ruleName];
        }
        references.Add(reference);
        References[ruleName] = references;
    }
    public void AddRuleReference(string ruleName, string referenced)
    {
        AddReference(ruleName, Reference.Rule(referenced));
    }

    public void AddTokenReference(string ruleName, string referenced)
    {
        AddReference(ruleName, Reference.Token(referenced));
    }

    public List<string> CheckReferences()
    {
        List<string> errors = new List<string>();
        
        foreach (var rule in References)
        {
            foreach (var reference in rule.Value)
            {
                if (ruleNames.All(x => x != reference.Name) && tokenNames.All(x => x != reference.Name)) 
                {
                    errors.Add($"rule {rule.Key} references {reference.Name} that does not exist.");
                }
            }
        }

        return errors;

    }
}