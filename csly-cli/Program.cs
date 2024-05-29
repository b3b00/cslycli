using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using CommandLine;
using decompiler;
using sly.cli.options;
using specificationExtractor;


namespace cslycli {

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<TestOptions, GenerateOptions, GenerateProjectOptions, ExtractOptions, DecompileOptions>(args)
            .MapResult(
                (TestOptions test) => { return Test(test); },
                (GenerateOptions generate) => { return Generate(generate); },
                (GenerateProjectOptions generateProject) => { return GenerateProject(generateProject); },
                (ExtractOptions extract) => { return Extract(extract);},
                (DecompileOptions decompile) => { return Decompile(decompile);},
                errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ToString());
                    }

                    return 1;
                }
            );


    }

    private static int Decompile(DecompileOptions decompile)
    {
        Decompiler decompiler = new Decompiler();
        var specification = decompiler.Decompile(decompile.LexerFqn, decompile.ParserFqn, decompile.AssemblyPath);
        
        if (File.Exists(decompile.SpecificationOutputFile))
        {
            File.Delete(decompile.SpecificationOutputFile);    
        }

        File.WriteAllText(decompile.SpecificationOutputFile, specification);

        return 0;
    }

    private static int Extract(ExtractOptions extract)
    {

        var extractor = new SpecificationExtractor();
        var specification = extractor.ExtractFromFiles(extract.LexerPath, extract.ParserPath);
        
        

        if (File.Exists(extract.SpecificationOutputFile))
        {
        File.Delete(extract.SpecificationOutputFile);    
        }

        File.WriteAllText(extract.SpecificationOutputFile, specification);

        return 0;
    }

    private static int Generate(GenerateOptions generate)
    {
        var fi = new FileInfo(generate.Grammar);
        var parserName = fi.Name.Replace(fi.Extension, "");
        var builder = new clsy.cli.builder.parser.ParserBuilder();

        

        var grammar = File.ReadAllText(generate.Grammar);

        var model = builder.CompileModel(grammar);
        if (model.IsError)
        {
            Console.WriteLine("Errors in grammar specification file:");
            foreach (var error in model.Error)
            {
                Console.WriteLine(error);
            }

            return 1;
        }

        var lexerModel = model.Value.LexerModel;
        var parserModel = model.Value.ParserModel;
        
        Console.WriteLine("Model compilation succeeded.");

        var lexerGenerator = new LexerGenerator();
        var enumCode = lexerGenerator.GenerateLexer(lexerModel, generate.NameSpace);
        var path = Path.Combine(generate.OutputDir, lexerModel.Name + ".cs");
        File.WriteAllText(path,enumCode);

        var parserGenerator = new ParserGenerator();
        var parserCode = parserGenerator.GenerateParser(model.Value,  generate.NameSpace, generate.ParserOutput);
        path = Path.Combine(generate.OutputDir, parserModel.Name + ".cs");
        File.WriteAllText(path,parserCode);
        return 0;
    }

    private static int GenerateProject(GenerateProjectOptions generate)
    {
        var fi = new FileInfo(generate.Grammar);
        var parserName = fi.Name.Replace(fi.Extension, "");
        var builder = new clsy.cli.builder.parser.ParserBuilder();
        var grammar = File.ReadAllText(generate.Grammar);

        var model = builder.CompileModel(grammar);
        if (model.IsError)
        {
            foreach (var error in model.Error)
            {
                Console.Error.WriteLine(error);    
            }
            return 1;
        }

        var parserModel = model.Value.ParserModel;
        var lexerModel = model.Value.LexerModel;

        Generate(new GenerateOptions() {Grammar = generate.Grammar, NameSpace = generate.NameSpace, ParserOutput = generate.ParserOutput, OutputDir = generate.OutputDir});
        
        string csproj = $@"<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net7.0</TargetFramework>
        <RootNamespace>{generate.NameSpace}</RootNamespace>                
        <PackageOutputPath>./nupkg</PackageOutputPath>
        <version>0.0.1</version>
        <PackageVersion>0.0.1</PackageVersion>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""sly"" Version=""3.1.4"" />
    </ItemGroup>

</Project>";
        File.WriteAllText(Path.Combine(generate.OutputDir,parserModel.Name)+".csproj",csproj);

        string extender =  lexerModel.HasExtension ? $"Extended{lexerModel.Name}.Extend{lexerModel.Name}": "null";
        
        string program = $@"
using sly.parser.generator;
using System;

namespace {generate.NameSpace} {{
    

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
        File.WriteAllText(Path.Combine(generate.OutputDir,"Program.cs"),program);
        
        return 0;
    }
    
    private static int Test(TestOptions test)
    {
        var fi = new FileInfo(test.Grammar);
        var parserName = fi.Name.Replace(fi.Extension, "");
        var builder = new clsy.cli.builder.parser.ParserBuilder();

        SyntaxTreeProcessor emptyProcessor = (Type type, Type lexerType, object tree) => { return ""; };

        var grammarSource = File.ReadAllText(test.Grammar);

        var mod = builder.CompileModel(grammarSource);
        if (mod.IsError)
        {
            Console.WriteLine("Errors in grammar specification file:");
            foreach (var error in mod.Error)
            {
                Console.WriteLine(error);
            }

            return 1;
        }

        Console.WriteLine("Model compilation succeeded.");

        List<(OutputFormat format, SyntaxTreeProcessor processor)> formatters =
            new List<(OutputFormat, SyntaxTreeProcessor)>();

        if (test.OutputTypes.Any())
        {
            formatters = test.OutputTypes.Select(x => (x.Value, (x == OutputFormat.DOT
                ? ((SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToDotGraph)
                : ((SyntaxTreeProcessor)ParserBuilder.SyntaxTreeToJson)))).ToList();
        }
        else
        {
            formatters = new List<(OutputFormat, SyntaxTreeProcessor)>() { (OutputFormat.NO, emptyProcessor) };
        }

        var grammar = File.ReadAllText(test.Grammar);
        var source = File.ReadAllText(test.Source);

        var result = builder.Getz(grammar,
            source, parserName,formatters.Select(x => (x.format.ToString(), x.processor)).ToList());

        if (result.IsError)
        {
            Console.WriteLine("Errors in source file:");
            foreach (var error in result.Error)
            {
                Console.WriteLine(error);
            }

            return 2;
        }

        Console.WriteLine("Parse succeeded.");

        if (test.OutputTypes != null && test.OutputTypes.Any())
        {
            foreach (var value in result.Value)
            {

                var outputFileExtension = value.format == OutputFormat.DOT.ToString() ? ".dot" : ".json";

                var outputFileName = Path.Combine(test.Output, parserName + outputFileExtension);
                if (File.Exists(outputFileName))
                {
                    File.Delete(outputFileName);
                }

                File.AppendAllText(outputFileName, value.content);

                Console.WriteLine($"file {outputFileName} generated.");
            }
        }

        return 0;
    }
    
    
}
}

   
