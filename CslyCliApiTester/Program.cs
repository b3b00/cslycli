// See https://aka.ms/new-console-template for more information



using csly_cli_api;

namespace csly_cli_api
{

    public class Program
    {

        public static void Main(string[] args)
        {
            GenerateAndBuild(args);
            Extract(args);
        }

        public static void Extract(string[] args)
        {
            var parser = File.ReadAllText($"C:/Users/olduh/dev/csly-cli/Generated/someParser.cs");
            var lexer = File.ReadAllText($"C:/Users/olduh/dev/csly-cli/Generated/someLexer.cs");
            var g = CslyProcessor.ExtractGrammar(parser, lexer);
            File.WriteAllText($"C:/Users/olduh/dev/csly-cli/Generated/grammar.txt", g);

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
            var grammar = @"
genericLexer someLexer;

[Mode(""default"", ""EXT"")]
@label(""en"",""integer"");
@label(""fr"",""entier"");
[Int] INT;
@label(""en"",""plus sign"");
@label(""fr"",""plus"");
[Sugar] PLUS : ""+""; 
@label(""en"",""addition"");
@label(""fr"",""addition"");
[KeyWord] ADD:""add"";
@label(""en"",""minus sign"");
@label(""fr"",""moins"");
[Sugar] MINUS : ""-"";
@label(""en"",""substraction"");
@label(""fr"",""soustraction"");
[KeyWord] REMOVE:""remove"";
@label(""en"",""times sign"");
@label(""fr"",""fois"");
[Sugar] TIMES : ""*"";
@label(""en"",""multiplication"");
@label(""fr"",""multiplication"");
[KeyWord] MUL:""mul"";
@label(""en"",""division sign"");
@label(""fr"",""diviser"");
[Sugar] SLASH : ""/""; 
@label(""en"",""division"");
@label(""fr"",""division""); 
[KeyWord] DIV:""div"";

@label(""en"",""test"");
@label(""fr"",""test"");
[Extension] TEST
>>>
-> '#'  -> ['0'-'9','A'-'F'] {6} -> END
<<<

[Mode(""default"", ""EXT"")]
[Push(""EXT"")]
[Sugar] OPEN : "">>>"";

[Mode(""EXT"")]
[Pop]
[Sugar] CLOSE : ""<<<"";


parser someParser;

-> root : someParser_expressions;

[Right 10] PLUS ADD;
@name(minus);
[Right 10] MINUS REMOVE;

[Right 50] TIMES MUL;
@name(div);
[Right 50] DIV SLASH;

#@name(prefixPlus);
[Prefix 100] PLUS ADD ""#"";

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

                var dot = CslyProcessor.GetDot(grammar, " 0 + ~1 + -2 div #3 1224 ");
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