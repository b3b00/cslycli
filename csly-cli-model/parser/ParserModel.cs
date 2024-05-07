namespace csly.cli.model.parser;

public class ParserModel : ICLIModel
{
    public List<Rule> Rules { get; set; }
    
    public bool UseMemoization { get; set; }
    
    public bool BroadenTokenWindow { get; set; }
    
    public bool AutoCloseIndentations { get; set; }
    
    public string Root => Rules.FirstOrDefault(x => x.IsRoot)?.NonTerminalName;
    public string Name { get; set; }
}