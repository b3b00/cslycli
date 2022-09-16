// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using clsy.cli.builder.parser.cli.model;
using csly.cli.model;
using csly.cli.parser;
using sly.buildresult;
using sly.lexer;
using sly.parser;
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
            var x = builder.BuildParser(model);
            // TODO ???
            Console.WriteLine("parser build");
            
            Console.WriteLine($"Parser<IN,OUT> : {x.parserType}");
            var buildResultType = typeof(BuildResult<object>);
            
                    
            var resultProperty = buildResultType.GetProperty("Result");
            var parser = resultProperty.GetValue(x.parserBuildResult, null);
            var resultPropertyGetter = resultProperty.GetMethod;
            // resultPropertyGetter = resultPropertyGetter.MakeGenericMethod(iLexerType);
            // var lexer = resultPropertyGetter.Invoke(lexerResult.lexerBuildResult, new object[] { });
            //var lexer = resultProperty.GetValue(lexerResult.lexerBuildResult);
            Console.WriteLine($"result.Result {parser.GetType()}");
        
            // var genLexType = typeof(GenericLexer<>);
            // var genGenLexType = genLexType.MakeGenericType(lexerResult.tokenType);
            // Console.WriteLine("GenericLexer<IN>");
            // var meth = genGenLexType.GetMethod("Tokenize", new Type[] { typeof(string) });
            // Console.WriteLine("tokenize");
            // var t = meth.Invoke(lexer, new object[] { "(a:=0; while a < 10 do (print a; a := a +1 ))" });
            // Console.WriteLine("called");
            //
            // Console.WriteLine(t);
            //
            // var lexResultType = typeof(LexerResult<>);
            // lexResultType = lexResultType.MakeGenericType(lexerResult.tokenType);
            // var dumpMethod = lexResultType.GetMethod("Dump", new Type[] { });
            // dumpMethod.Invoke(t, new object[]{});
        }
        else
        {
            Console.WriteLine("bof");
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