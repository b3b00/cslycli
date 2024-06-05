// See https://aka.ms/new-console-template for more information



using csly_cli_api;

namespace csly_cli_api
{

    public class Program
    {

        public static void Main(string[] args)
        {
            //Extract(args);
            GenerateAndBuild(args);
        }

        public static void Extract(string[] args)
        {
            // var parser = File.ReadAllText($"C:/Users/olduh/dev/csly-cli/Generated/someParser.cs");
            // var lexer = File.ReadAllText($"C:/Users/olduh/dev/csly-cli/Generated/someLexer.cs");
            var lexer = File.ReadAllText($"C:/Users/olduh/dev/csly/src/samples/SimpleTemplate/TemplateLexer.cs");
            var parser = File.ReadAllText($"C:/Users/olduh/dev/csly/src/samples/SimpleTemplate/TemplateParser.cs");
            var g = CslyProcessor.ExtractGrammar(parser, lexer);
            File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/grammar.txt", g.Result);

            var gr = CslyProcessor.Compile(g);
            if (gr.IsOK)
            {
                Console.WriteLine("recompiled grammar is ok");
            }
            else
            {
                foreach (var error in gr.Errors)
                {
                    Console.WriteLine(error);
                }
            }
        }

        public static void GenerateAndBuild(string[] args)
        {
            var grammar = File.ReadAllText("C:/Users/olduh/dev/csly-cli/Generated/grammar.txt");

            string source = @"
1 / 2 / 3 + 4
";

            var r = CslyProcessor.GenerateParser(grammar,"ns","object");

            if (r.IsOK)
            {
                File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/{r.Result.ParserName}.cs", r.Result.Parser);
                File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/{r.Result.LexerName}.cs", r.Result.Lexer);
                //Console.WriteLine(r.Result.Parser);
                var templatesource = @"hello-{=world=}
-billy-
{% if (a == 1) %}
-bob-
{%else%}
-boubou-
{%endif%}
this is the end";
                var dot = CslyProcessor.GetDot(grammar, templatesource);
                if (dot.IsOK)
                {
                    Console.WriteLine("parse is OK");
                    Console.WriteLine(dot.Result);
                }
                else
                {
                    foreach (var error in dot.Errors)
                    {
                        Console.WriteLine(error);
                    }
                }
            }
            else
            {
                foreach (var error in r.Errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}