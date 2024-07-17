using System;
using System.Collections.Generic;
using System.Linq;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using csly.cli.model;
using csly.cli.model.tree;
using specificationExtractor;

namespace csly_cli_api;

public class CslyProcessor : ICslyProcessor
{

    private ParserBuilder _parserBuilder;
    
    public CslyProcessor()
    {
        _parserBuilder = new ParserBuilder();
    }
    
    /// <summary>
    /// Compiles and check a grammar specification 
    /// </summary>
    /// <param name="grammar">the grammar specification</param>
    /// <returns></returns>
    public CliResult<Model> CompileModel(string grammar)
    {
        
        Chrono chrono = new Chrono();
        chrono.Start();
        var model = _parserBuilder.CompileModel(grammar, "MinimalParser",chrono);
        chrono.Stop();
        if (model.IsOk)
        {
            var result = new CliResult<Model>(model.Value);
            result.Timings = chrono.LabeledElapsedMilliseconds;
            return model.Value;
        }
        return model.Error;
    }

    public CliResult<string> Compile(string grammar)
    {
        Chrono chrono = new Chrono();
        chrono.Start();
        var buildResult = _parserBuilder.Compile(grammar, "MinimalParser",chrono);
        chrono.Stop();
        if (buildResult.IsOk)
        {
            var result = new CliResult<string>(buildResult.Value);
            result.Timings = chrono.LabeledElapsedMilliseconds;
            return buildResult.Value;
        }
        return buildResult.Error;
    }

    /// <summary>
    /// Parses a source according to a grammar specification
    /// </summary>
    /// <param name="grammar">The grammar specification</param>
    /// <param name="source">The source</param>
    /// <returns></returns>
    public CliResult<string> Parse(string grammar, string source)
    {
        var chrono = new Chrono();
        chrono.Start();
        var model = _parserBuilder.CompileModel(grammar, "Dummy__@@_4589_Parser", chrono);
        
        if (model.IsOk)
        {
            var r = _parserBuilder.Parse(grammar, source, model.Value.ParserModel.Name, model.Value.ParserModel.Root, chrono);
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
    public CliResult<string> GetDot(string grammar, string source)
    {
        var chrono = new Chrono();
        var model = _parserBuilder.CompileModel(grammar, "MinimalParser", chrono);
        if (model.IsOk)
        {
            var r = _parserBuilder.Getz(grammar, source, model.Value.ParserModel.Name,
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
    /// Returns a mermaid js flow chart representation of the syntax tree of a source (according to a grammar spec)
    /// </summary>
    /// <param name="grammar">Grammar spec</param>
    /// <param name="source">source</param>
    /// <returns></returns>
   
    
    public CliResult<string> GetMermaid(string grammar, string source)
    {
        var chrono = new Chrono();
        var model = _parserBuilder.CompileModel(grammar, "MinimalParser", chrono);
        if (model.IsOk)
        {
            var r = _parserBuilder.Getz(grammar, source, model.Value.ParserModel.Name,
                new List<(string format, SyntaxTreeProcessor processor)>()
                    { ("DOT", (SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToMermaid) },model.Value.ParserModel.Root, chrono);
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
    public CliResult<string> GetJson(string grammar, string source)
    {
        
        var chrono = new Chrono();
        var model = _parserBuilder.CompileModel(grammar, "MinimalParser",chrono);
        if (model.IsOk)
        {
            var r = _parserBuilder.Getz(grammar, source, "TestParser",
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
    public CliResult<GeneratedSource> GenerateParser(string grammar, string nameSpace, string outputType)
    {
        Chrono chrono = new Chrono();
        
        var model = _parserBuilder.CompileModel(grammar, "MinimalParser", chrono);
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
        <TargetFramework>net8.0</TargetFramework>
        <RootNamespace>{nameSpace}</RootNamespace>                
        <PackageOutputPath>./nupkg</PackageOutputPath>
        <version>0.0.1</version>
        <PackageVersion>0.0.1</PackageVersion>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""sly"" Version=""3.2.7"" />
    </ItemGroup>

</Project>";
            chrono.Tick("project source generation");
            
            string extender =  lexerModel.HasExtension ? $"Extended{lexerModel.Name}.Extend{lexerModel.Name}": "null";
            
            string program = $@"
using sly.parser.generator;
using System;

namespace {nameSpace} {{
    

    public class Program {{
        public static void Main(string[] args) {{
            var builder = new ParserBuilder<{lexerModel.Name}, object>();
            var instance = new {parserModel.Name}();

            var buildParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, null,{extender});
            if (buildParser.IsOk)
            {{
                var result = buildParser.Result.Parse(""<< HERE COMES YOUR SOURCE"");
                if (result.IsOk)
                {{
                    Console.WriteLine(result.Result);
                }}
                else
                {{
                    foreach (var error in result.Errors)
                    {{
                        Console.WriteLine(error.ErrorMessage);
                    }}
                }}

            }}
        }}
    }}
}}
";
            
            return new CliResult<GeneratedSource>(new GeneratedSource(model.Value.LexerModel.Name, lexerSource, model.Value.ParserModel.Name, parserSource, csproj, program));
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
    public CliResult<string> ExtractGrammar(string parser, string lexer)
    {
        var extractor = new SpecificationExtractor();
        var grammar  = extractor.ExtractFromSource(lexer, parser);
        return new CliResult<string>(grammar);

    }

    public CliResult<ISyntaxNode> GetSyntaxTree(string grammar, string source)
    {
        var chrono = new Chrono();
        var model = _parserBuilder.CompileModel(grammar, "MinimalParser", chrono);
        if (model.IsOk)
        {
            var r = _parserBuilder.GetSyntaxTree(grammar, source, model.Value.ParserModel.Name,
                model.Value.ParserModel.Root,
                chrono: chrono);
            if (r.IsError)
            {
                return new CliResult<ISyntaxNode>(r.Error.Select(x => $"parse error : {x}").ToList());
            }

            return new CliResult<ISyntaxNode>(r.Value);
        }

        return new CliResult<ISyntaxNode>(model.Error.Select(x => $"grammar error : {x}").ToList());

    }
}