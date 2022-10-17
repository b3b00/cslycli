using System;
using System.IO;
using clsy.cli.builder.parser;
using CommandLine;
using sly.cli.options;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<TestOPtions, GenerateOPtions>(args)
            .MapResult(
                (TestOPtions test) => { return Test(test); },
                (GenerateOPtions generate) => { return 0; },
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

    private static int Test(TestOPtions test)
    {
        var fi = new FileInfo(test.Grammar);
        var parserName = fi.Name.Replace(fi.Extension,"");
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

        SyntaxTreeProcessor processor = test.OUtputType.HasValue
            ? (test.OUtputType == OutputFormat.DOT
                ? ParserBuilder.SyntaxTreeToDotGraph
                : ParserBuilder.SyntaxTreeToJson)
            : emptyProcessor;

        var result = builder.Get(test.Grammar,
            test.Source, processor);

        if (result.IsError)
        {
            foreach (var error in result.Error)
            {
                Console.WriteLine(error);
            }

            return 2;
        }

        Console.WriteLine("Parse succeeded.");

        if (test.OUtputType.HasValue)
        {

            var outputFileExtension = test.OUtputType == OutputFormat.DOT ? ".dot" : ".json";

            var dotFileName = Path.Combine(test.Output, parserName + outputFileExtension);
            if (File.Exists(dotFileName))
            {
                File.Delete(dotFileName);
            }

            File.AppendAllText(dotFileName, result.Value);
        }

        return 0;
    }

}
   
