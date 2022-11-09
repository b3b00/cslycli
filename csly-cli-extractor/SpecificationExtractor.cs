using System.Diagnostics.CodeAnalysis;

namespace specificationExtractor;

public class SpecificationExtractor
{

    private LexerSpecificationExtractor _lexerSpecificationExtractor;

    private ParserSpecificationExtractor _parserSpecificationExtractor;
    
    public SpecificationExtractor()
    {
        _lexerSpecificationExtractor = new LexerSpecificationExtractor();
        _parserSpecificationExtractor = new ParserSpecificationExtractor();
    }

    [ExcludeFromCodeCoverage]
    public string ExtractFromFiles(string lexerFileName, string parserFileName)
    {
        var lexerSource = File.ReadAllText(lexerFileName);
        var parserSource = File.ReadAllText(parserFileName);
        return ExtractFromSource(lexerSource, parserSource);
    }

    public string ExtractFromSource(string lexerSource, string parserSource)
    {
        
        var parserSpec = _parserSpecificationExtractor.ExtractFromSource(parserSource);
        
        var lexerSpec = _lexerSpecificationExtractor.ExtractFromSource(lexerSource);

        return $@"
{lexerSpec}

{parserSpec}";
    }
}