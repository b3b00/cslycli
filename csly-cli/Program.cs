// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using clsy.cli.builder.parser.cli.model;
using csly.cli.model;
using csly.cli.parser;
using sly.lexer;
using sly.parser.generator;

public class Program
{
    public static void Main(string[] args)
    {
        var model = TestParser();
        if (model != null)
        {
            //csly.cli.builder.parser.ParserBuilder.BuildParser(model);
            var builder = new clsy.cli.builder.parser.ParserBuilder();
            builder.BuildParser(model);
        }
        else
        {
            Console.WriteLine("bof");
        }
        // TestLexer();
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