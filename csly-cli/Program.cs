using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using CommandLine;
using sly.cli.options;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<TestOptions, GenerateOPtions>(args)
            .MapResult(
                (TestOptions test) => { return Test(test); },
                (GenerateOPtions generate) => { return Generate(generate); },
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

    private static int Generate(GenerateOPtions generate)
    {
        var fi = new FileInfo(generate.Grammar);
        var parserName = fi.Name.Replace(fi.Extension, "");
        var builder = new clsy.cli.builder.parser.ParserBuilder();

        



        var model = builder.CompileModel(generate.Grammar);
        if (model.IsError)
        {
            foreach (var error in model.Error)
            {
                Console.WriteLine(error);
            }

            return 1;
        }
        var enumCode = LexerGenerator.GenerateLexer(model.Value.LexerModel, generate.Lexer, generate.NameSpace);
        var path = Path.Combine(fi.Directory.FullName, generate.Lexer + ".cs");
        File.WriteAllText(path,enumCode);
        

        var parserCode = ParserGenerator.GenerateParser(model.Value.ParserModel, generate.Parser, generate.NameSpace, generate.Lexer, generate.ParserOutput);
        path = Path.Combine(fi.Directory.FullName, generate.Parser + ".cs");
        File.WriteAllText(path,parserCode);
        return 0;
    }

    private static int Test(TestOptions test)
    {
        var fi = new FileInfo(test.Grammar);
        var parserName = fi.Name.Replace(fi.Extension, "");
        var builder = new clsy.cli.builder.parser.ParserBuilder();

        SyntaxTreeProcessor emptyProcessor = (Type type, Type lexerType, object tree) => { return ""; };



        var mod = builder.CompileModel(test.Grammar);
        if (mod.IsError)
        {
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


        var result = builder.Getz(test.Grammar,
            test.Source, formatters.Select(x => (x.format.ToString(), x.processor)).ToList());

        if (result.IsError)
        {
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


   
