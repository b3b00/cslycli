namespace csly.cli.model.parser;

public class ParserModel : ICLIModel
{
    public List<Rule> Rules { get; set; }
    
    
    public string Root => Rules.FirstOrDefault(x => x.IsRoot)?.NonTerminalName;
    public string Name { get; set; }
}