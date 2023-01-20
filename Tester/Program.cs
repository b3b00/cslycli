// See https://aka.ms/new-console-template for more information

using System.Reflection;
using clsy.cli.builder.parser;
using csly.cli.model;
using csly.cli.parser;
using sly.lexer;
using sly.parser.generator;

public static class Program
{

    public static List<string> GetLines(this string content)
    {
        List<string> lines = new List<string>();
        using (StringReader reader = new StringReader(content))
        {
            string line = reader.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = reader.ReadLine();
            }
        }

        return lines;
    }

    public static void Main(string[] args)
    {

        var grammar = @"

genericLexer ExtensionLexer;

[Extension] TEST
>>>
-> '#'  -> ['0'-'9','A'-'F'] {6} -> END
<<<


parser ExtensionParser;

-> root : TEST;
";


        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "ExtensionParser");
        var json = builder.Getz(grammar, "#FF00FF", "ExtensionParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        if (json.IsError)
        {
            foreach (var s in json.Error)
            {
                Console.WriteLine(s);
            }
        }
        else
        {
            var content = json.Value.First().content;
            Console.WriteLine(content);
        }
        ;
    }
}
