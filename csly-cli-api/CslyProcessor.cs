using clsy.cli.builder;
using clsy.cli.builder.parser;
using csly.cli.model;
using specificationExtractor;

namespace csly_cli_api;

public class CslyProcessor
{

    /// <summary>
    /// Compiles and check a grammar specification 
    /// </summary>
    /// <param name="grammar">the grammar specification</param>
    /// <returns></returns>
    public static CliResult<Model> Compile(string grammar)
    {
        var builder = new ParserBuilder();
        Chrono chrono = new Chrono();
        chrono.Start();
        var model = builder.CompileModel(grammar, "MinimalParser",chrono);
        chrono.Stop();
        if (model.IsOk)
        {
            var result = new CliResult<Model>(model.Value);
            result.Timings = chrono.LabeledElapsedMilliseconds;
            return model.Value;
        }
        return model.Error;
    }

    /// <summary>
    /// Parses a source according to a grammar specification
    /// </summary>
    /// <param name="grammar">The grammar specification</param>
    /// <param name="source">The source</param>
    /// <returns></returns>
    public static CliResult<string> Parse(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var chrono = new Chrono();
        chrono.Start();
        var model = builder.CompileModel(grammar, "Dummy__@@_4589_Parser", chrono);
        
        if (model.IsOk)
        {
            var r = builder.Parse(grammar, source, model.Value.ParserModel.Name, model.Value.ParserModel.Root, chrono);
            chrono.Tick("source parsing");
            if (r.IsError)
            {
                return new CliResult<string>(r.Error.Select(x => $"parse error : {x}").ToList())
                {
                    Timings = chrono.LabeledElapsedMilliseconds
                };
            }
            else
            {
                return new CliResult<string>(r.Value) {
                    Timings = chrono.LabeledElapsedMilliseconds
                };
            }
        }
        else
        {
            return new CliResult<string>(model.Error.Select(x => $"grammar error : {x}").ToList()) {
                Timings = chrono.LabeledElapsedMilliseconds
            };
        }
    }
    
    /// <summary>
    /// Returns a graphviz dot representation of the syntax tree of a source (according to a grammar spec)
    /// </summary>
    /// <param name="grammar">Grammar spec</param>
    /// <param name="source">source</param>
    /// <returns></returns>
    public static CliResult<string> GetDot(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var chrono = new Chrono();
        var model = builder.CompileModel(grammar, "MinimalParser", chrono);
        if (model.IsOk)
        {
            var r = builder.Getz(grammar, source, model.Value.ParserModel.Name,
                new List<(string format, SyntaxTreeProcessor processor)>()
                    { ("DOT", (SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToDotGraph) },model.Value.ParserModel.Root, chrono);
            if (r.IsError)
            {
                chrono.Stop();
                return new CliResult<string>(r.Error.Select(x => $"parse error : {x}").ToList()) {
                    Timings = chrono.LabeledElapsedMilliseconds
                };
            }
            else
            {
                chrono.Stop();
                return new CliResult<string>(r.Value[0].content)
                {
                    Timings = chrono.LabeledElapsedMilliseconds
                };
            }
        }
        else
        {
            chrono.Stop();
            return new CliResult<string>(model.Error.Select(x => $"grammar error : {x}").ToList()) {
                Timings = chrono.LabeledElapsedMilliseconds
            };
        }
    }
   
    /// <summary>
    /// Returns a json representation of the syntax tree of a source (according to a grammar spec)
    /// </summary>
    /// <param name="grammar">Grammar spec</param>
    /// <param name="source">source</param>
    /// <returns></returns>
    public static CliResult<string> GetJson(string grammar, string source)
    {
        var builder = new ParserBuilder();
        var chrono = new Chrono();
        var model = builder.CompileModel(grammar, "MinimalParser",chrono);
        if (model.IsOk)
        {
            var r = builder.Getz(grammar, source, "TestParser",
                new List<(string format, SyntaxTreeProcessor processor)>()
                    { ("JSON", (SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToJson) }, model.Value.ParserModel.Root,chrono);
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

    /// <summary>
    /// generates C# source for lexer and parse according to a grammar spec.
    /// </summary>
    /// <param name="grammar">grammar spec</param>
    /// <param name="nameSpace">namespace of the generated source</param>
    /// <param name="outputType">expected output type for the parser</param>
    /// <returns></returns>
    public static CliResult<GeneratedSource> GenerateParser(string grammar, string nameSpace, string outputType)
    {
        Chrono chrono = new Chrono();
        
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser", chrono);
        if (model.IsOk)
        {
            string lexerSource = "";
            string parserSource = "";
            
            var lexerModel = model.Value.LexerModel;
            var parserModel = model.Value.ParserModel;
        
            Console.WriteLine("Model compilation succeeded.");

            var lexerGenerator = new LexerGenerator();
            lexerSource = lexerGenerator.GenerateLexer(lexerModel, nameSpace);
            chrono.Tick("lexer source generation");

            var parserGenerator = new ParserGenerator();
            parserSource = parserGenerator.GenerateParser(model.Value, nameSpace, outputType);
            chrono.Tick("parser source generation");
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
            chrono.Tick("project source generation");
            return new CliResult<GeneratedSource>(new GeneratedSource(model.Value.LexerModel.Name, lexerSource, model.Value.ParserModel.Name, parserSource, csproj));
        }
        else
        {
            return new CliResult<GeneratedSource>(model.Error.Select(x => $"grammar error : {x}").ToList());
        }
    }

    /// <summary>
    /// Extracts a grammar spec from C# lexer and parser source file 
    /// </summary>
    /// <param name="parser">parser source file content</param>
    /// <param name="lexer">lexer source file content</param>
    /// <returns></returns>
    public static CliResult<string> ExtractGrammar(string parser, string lexer)
    {
        var extractor = new SpecificationExtractor();
        var grammar  = extractor.ExtractFromSource(lexer, parser);
        return new CliResult<string>(grammar);

    }
}