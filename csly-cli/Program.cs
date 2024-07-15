using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using CommandLine;
using csly_cli_api;
using csly.cli.model.tree;
using decompiler;
using sly.cli.options;
using specificationExtractor;


namespace cslycli {

public class Program
{
    public static void Main(string[] args)
    {
        var processor = new CslyProcessor();
        Parser.Default.ParseArguments<TestOptions, GenerateOptions, GenerateProjectOptions, ExtractOptions, DecompileOptions>(args)
            .MapResult(
                (TestOptions test) => { return Test(test, processor); },
                (GenerateOptions generate) => { return Generate(generate, processor); },
                (GenerateProjectOptions generateProject) => { return Generate(generateProject.ToGenerateOptions(), processor, true); },
                (ExtractOptions extract) => { return Extract(extract, processor);},
                (DecompileOptions decompile) => { return Decompile(decompile, processor);},
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

    private static int Decompile(DecompileOptions decompile, CslyProcessor processor)
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

    private static int Extract(ExtractOptions extract, CslyProcessor processor)
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

    private static int Generate(GenerateOptions generate, CslyProcessor processor, bool generateProject = false)
    {
        var fi = new FileInfo(generate.Grammar);
        var parserName = fi.Name.Replace(fi.Extension, "");
        var builder = new clsy.cli.builder.parser.ParserBuilder();



        var grammar = File.ReadAllText(generate.Grammar);

        var compilationResult = processor.Compile(grammar);
        if (compilationResult.IsError)
        {
            Console.WriteLine("Errors in grammar specification file:");
            foreach (var error in compilationResult.Errors)
            {
                Console.WriteLine(error);
            }

            return 1;
        }

        var generationResult = processor.GenerateParser(grammar, generate.NameSpace, generate.ParserOutput);

        if (generationResult.IsOK)
        {

            var path = Path.Combine(generate.OutputDir, generationResult.Result.LexerName + ".cs");
            File.WriteAllText(path, generationResult.Result.Lexer);
            
            path = Path.Combine(generate.OutputDir, generationResult.Result.ParserName + ".cs");
            File.WriteAllText(path,generationResult.Result.Parser);
            
            if (generateProject)
            {
                File.WriteAllText(Path.Combine(generate.OutputDir,generationResult.Result.ParserName)+".csproj",generationResult.Result.Project);
                File.WriteAllText(Path.Combine(generate.OutputDir,"Program.cs"),generationResult.Result.Program);
            }
            return 0;
        }
        else
        {
            Console.WriteLine("Errors while generating source code:");
            foreach (var error in generationResult.Errors)
            {
                Console.WriteLine(error);
            }

            return 2;
        }

       


    }

    private static int Test(TestOptions test, CslyProcessor processor)
    {
        var fi = new FileInfo(test.Grammar);
        var parserName = fi.Name.Replace(fi.Extension, "");
        var builder = new clsy.cli.builder.parser.ParserBuilder();

        SyntaxTreeProcessor emptyProcessor = (Type type, Type lexerType, ISyntaxNode tree) => { return ""; };

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
            var formats = new Dictionary<OutputFormat, SyntaxTreeProcessor>()
            {
                { OutputFormat.DOT, ParserBuilder.SyntaxTreeToDotGraph },
                { OutputFormat.JSON, ParserBuilder.SyntaxTreeToJson },
                { OutputFormat.MERMAID, ParserBuilder.SyntaxTreeToMermaid }
            };
            
            formatters = test.OutputTypes
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Select(x => (x,formats[x]))
                .ToList();
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

                var extensions = new Dictionary<string, string>()
                {
                    { nameof(OutputFormat.DOT), ".dot" },
                    { nameof(OutputFormat.MERMAID), ".mermaid" },
                    { nameof(OutputFormat.JSON), ".json" }
                };

                var outputFileExtension = extensions[value.format];

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

   
