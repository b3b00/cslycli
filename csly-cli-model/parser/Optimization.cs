using sly.parser.generator;

namespace csly.cli.model.parser;

public class Optimization : ICLIModel
{
    public bool UseMemoization { get; set; }
    public bool BroadenTokenWindow { get; set; }
    
}