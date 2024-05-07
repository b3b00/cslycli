// See https://aka.ms/new-console-template for more information



using csly_cli_api;

namespace csly_cli_api
{

    public class Program
    {

        public static void Main(string[] args)
        {
            var grammar = @"
genericLexer MinimalLexer;
[Date] DATE : YYYYMMDD '-';

parser MinimalParser;

-> root : DATE ;
";

            string source = @"
2024-05-07
";


            var r = CslyProcessor.GetDot(grammar, source);
            if (r.IsOK)
            {
                Console.WriteLine(r.Result);
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