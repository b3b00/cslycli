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

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<TestOptions, GenerateOptions, ExtractOptions, DecompileOptions>(args)
            .MapResult(
                (TestOptions test) => { return Test(test); },
                (GenerateOptions generate) => { return Generate(generate); },
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
        
        Console.WriteLine("Model compilation succeeded.");

        var lexerGenerator = new LexerGenerator();
        var enumCode = lexerGenerator.GenerateLexer(model.Value.LexerModel, generate.NameSpace);
        var path = Path.Combine(fi.Directory.FullName, model.Value.LexerModel.Name + ".cs");
        File.WriteAllText(path,enumCode);

        var parserGenerator = new ParserGenerator();
        var parserCode = parserGenerator.GenerateParser(model.Value,  generate.NameSpace, generate.ParserOutput);
        path = Path.Combine(fi.Directory.FullName, model.Value.ParserModel.Name + ".cs");
        File.WriteAllText(path,parserCode);
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
    
    
    // private static int TestGen(TestGenerate testGen)
    // {
    //     my.MyParser instance = new my.MyParser();
    //     var builder = new ParserBuilder<my.MyLexer, object>();
    //     var Parser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
    //     
    //     if (Parser.IsOk)
    //     {
    //         var source = File.ReadAllText(testGen.Source);
    //         var r =Parser.Result.Parse(source);
    //         if (r.IsError)
    //         {
    //             foreach (var error in r.Errors)
    //             {
    //                 Console.WriteLine(error.ErrorMessage);
    //             }
    //         }
    //         else
    //         {
    //             Console.WriteLine("parse OK !");
    //             Console.WriteLine("result :: "+r.Result);
    //         }
    //     }
    //     else
    //     {
    //         foreach (var error in Parser.Errors)
    //         {
    //             Console.WriteLine(error.Message);
    //         }
    //     }
    //     return 0;
    // }
}


   
