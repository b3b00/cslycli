// See https://aka.ms/new-console-template for more information



using csly_cli_api;

namespace csly_cli_api
{

    public class Program
    {

        public static void Main(string[] args)
        {
            var grammar = @"
genericLexer MissingReferenceLexer;

[Int] INT;

parser MissingReferenceParser;

root : INT;

missingNt : toto;

missingT : POUET;

missingChoice : [missingT|missingNt|missingReferenceInChoice];

missingGroup : (missingT INT missingInGroup); 
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