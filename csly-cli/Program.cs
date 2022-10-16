// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using clsy.cli.builder.parser.cli.model;
using CommandLine;
using csly.cli.model;
using csly.cli.parser;
using sly.buildresult;
using sly.cli.options;
using sly.lexer;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.generator.visitor.dotgraph;
using sly.parser.syntax.tree;

public class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<TestOPtions, GenerateOPtions>(args)
            .MapResult(
                (TestOPtions test) => { return 0; },
                (GenerateOPtions generate) => { return 0;},
                errors => {   foreach (var error in errors)
                    {
                        Console.WriteLine(error.ToString());
                    }
                    return 1;}
            );

        Test();
    }

    private static void Test(TestOPtions test)
    {
        var fi = new FileInfo(test.Grammar);
        var parserName = fi.Name;
        var builder = new clsy.cli.builder.parser.ParserBuilder();
        

        if (test.HasOtput)
        {
            if (test.OUtputType == OutputFormat.DOT)
            {
                var graph = builder.GetGraphVizDot(test.Grammar,
                    test.Source);
            

                var dotFileName = Path.Combine(test.Output, parserName + ".dot");
                if (File.Exists(dotFileName))
                {
                    File.Delete(dotFileName);
                }

                File.AppendAllText(dotFileName, graph.Compile());
            }
            else
            {
                var jsonFileName = Path.Combine(test.Output, parserName + ".json");
                var json = builder.GetJsonSerialization(test.Grammar,
                    test.Source);
            
                if (File.Exists(jsonFileName))
                {
                    File.Delete(jsonFileName);
                }

                File.AppendAllText(jsonFileName, json);
            }

            
        }
    }


    private static Model TestParser()
    {
        ParserBuilder<CLIToken, ICLIModel> builder = new ParserBuilder<CLIToken, ICLIModel>();
        var instance = new CLIParser();
        //TestLexer();

        var buildParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        if (buildParser.IsOk)
        {
            var content = File.ReadAllText(@"C:\Users\olduh\dev\csly-cli\csly-cli\test.txt");
            var result = buildParser.Result.ParseWithContext(content, new ParserContext());
            if (result.IsError)
            {
                result.Errors.ForEach(x => Console.WriteLine(x.ErrorMessage));
            }
            else
            {
                Model model = result.Result as Model;
                model.LexerModel.Tokens.ForEach(Console.WriteLine);
                return model;
            }
        }
        else
        {
            buildParser.Errors.ForEach(x => Console.WriteLine(x.Message));
        }

        return null;
    }

    private static void TestLexer()
    {
        var lexerBuildResult = LexerBuilder.BuildLexer<CLIToken>();

        if (lexerBuildResult.IsOk)
        {
            var generic = lexerBuildResult.Result as GenericLexer<CLIToken>;
            var graph = generic.ToGraphViz();

            var content = File.ReadAllText(@"C:\Users\olduh\dev\csly-cli\csly-cli\test.txt");
            var tokens = lexerBuildResult.Result.Tokenize(content);
            
            if (tokens.IsOk)
            {
                foreach (var token in tokens.Tokens)
                {
                    Console.WriteLine(token);
                }
            }
            else
            {
                Console.WriteLine(tokens.Error.ErrorMessage);
            }
        }
        else
        {
            lexerBuildResult.Errors.ForEach(x => Console.WriteLine(x.Message));
        }
    }
}