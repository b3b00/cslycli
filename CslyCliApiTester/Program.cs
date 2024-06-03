// See https://aka.ms/new-console-template for more information



using csly_cli_api;

namespace csly_cli_api
{

    public class Program
    {

        public static void Main(string[] args)
        {
            var grammar = @"
genericLexer LeftRecursiveLexer;

[Int] INT;
[Double] DO;
 [Sugar] COMMA : "","";

parser LeftRecursiveParser;

-> root : item;

-> item : item (COMMA item)*;

-> item : [INT|D];

";

            string source = @"
1 / 2 / 3 + 4
";

            var r = CslyProcessor.GenerateParser(grammar,"ns","int");

            if (r.IsOK)
            {
                File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/{r.Result.ParserName}.cs", r.Result.Parser);
                File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/{r.Result.LexerName}.cs", r.Result.Lexer);
                //Console.WriteLine(r.Result.Parser);
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