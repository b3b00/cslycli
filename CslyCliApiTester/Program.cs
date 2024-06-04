// See https://aka.ms/new-console-template for more information



using csly_cli_api;

namespace csly_cli_api
{

    public class Program
    {

        public static void Main(string[] args)
        {
            var grammar = @"
genericLexer someLexer;

[Int] INT;
[Sugar] PLUS : ""+""; 
[KeyWord] AND:""and"";
[Sugar] MINUS : ""-"";
[KeyWord] REMOVE:""remove"";
[Sugar] TIMES : ""*"";
[KeyWord] MUL:""mul"";
[Sugar] SLASH : ""/"";  
[KeyWord] DIV:""div"";


parser someParser;

-> root : someParser_expressions;

[Right 10] PLUS ADD;
@name(minus);
[Right 10] MINUS REMOVE;

[Right 50] TIMES MUL;
@name(div);
[Right 50] DIV SLASH;

@name(prefixPlus);
[Prefix 100] PLUS ADD;

[Prefix 100] MINUS REMOVE ""~"";


[Operand] 
integer : INT;

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