using clsy.cli.builder;
using clsy.cli.builder.parser;
using csly.cli.model;
using specificationExtractor;

namespace csly_cli_api;

public class CslyProcessor
{

    public static CliResult<Model> Compile(string grammar)
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            return model.Value;
        }
        return model.Error;
    }

    public static CliResult<string> Parse(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            var r = builder.Parse(grammar, source, model.Value.ParserModel.Name, model.Value.ParserModel.Root);
            if (r.IsError)
            {
                return new CliResult<string>(r.Error.Select(x => $"parse error : {x}").ToList());
            }
            else
            {
                return new CliResult<string>(r.Value);
            }
        }
        else
        {
            return new CliResult<string>(model.Error.Select(x => $"grammar error : {x}").ToList());
        }
    }
    
    public static CliResult<string> GetDot(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            var r = builder.Getz(grammar, source, model.Value.ParserModel.Name,
                new List<(string format, SyntaxTreeProcessor processor)>()
                    { ("DOT", (SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToDotGraph) },model.Value.ParserModel.Root);
            if (r.IsError)
            {
                return new CliResult<string>(r.Error.Select(x => $"parse error : {x}").ToList());
            }
            else
            {
                return new CliResult<string>(r.Value[0].content);
            }
        }
        else
        {
            return new CliResult<string>(model.Error.Select(x => $"grammar error : {x}").ToList());
        }
    }
   
    public static CliResult<string> GetJson(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            var r = builder.Getz(grammar, source, "TestParser",
                new List<(string format, SyntaxTreeProcessor processor)>()
                    { ("JSON", (SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToJson) });
            if (r.IsError)
            {
                return new CliResult<string>(r.Error.Select(x => $"parse error : {x}").ToList());
            }
            else
            {
                return new CliResult<string>(r.Value[0].content);
            }
        }
        else
        {
            return new CliResult<string>(model.Error.Select(x => $"grammar error : {x}").ToList());
        }
    }

    public static CliResult<GeneratedSource> GenerateParser(string grammar, string nameSpace, string outputType)
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            string lexerSource = "";
            string parserSource = "";
            
            var lexerModel = model.Value.LexerModel;
            var parserModel = model.Value.ParserModel;
        
            Console.WriteLine("Model compilation succeeded.");

            var lexerGenerator = new LexerGenerator();
            lexerSource = lexerGenerator.GenerateLexer(lexerModel, nameSpace);

            var parserGenerator = new ParserGenerator();
            parserSource = parserGenerator.GenerateParser(model.Value, nameSpace, outputType);
            
            string csproj = $@"<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net7.0</TargetFramework>
        <RootNamespace>{nameSpace}</RootNamespace>                
        <PackageOutputPath>./nupkg</PackageOutputPath>
        <version>0.0.1</version>
        <PackageVersion>0.0.1</PackageVersion>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""sly"" Version=""3.1.6"" />
    </ItemGroup>

</Project>";
            
            return new CliResult<GeneratedSource>(new GeneratedSource(model.Value.LexerModel.Name, lexerSource, model.Value.ParserModel.Name, parserSource, csproj));
        }
        else
        {
            return new CliResult<GeneratedSource>(model.Error.Select(x => $"grammar error : {x}").ToList());
        }
    }

    public static CliResult<string> ExtractGrammar(string parser, string lexer)
    {
        var extractor = new SpecificationExtractor();
        var grammar  = extractor.ExtractFromSource(lexer, parser);
        return new CliResult<string>(grammar);

    }
}