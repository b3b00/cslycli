// See https://aka.ms/new-console-template for more information

using System.Reflection;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using csly.cli.model;
using csly.cli.parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cslyCliTester {

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
        TestDate();
    }

   

    private static void TestDate()
    {
        var grammar = @"
genericLexer MinimalLexer;
[Date] DATE : YYYYMMDD '.';

parser MinimalParser;

-> root : DATE ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        if (model.IsOk)
        {
            Console.WriteLine("model is OK");
        }
        else
        {
            foreach (var error in model.Error)
            {
                Console.WriteLine(error);
            }
        }
        
        var json = builder.Getz(grammar, "2024.04.23", "MyDateParser", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)});
        if (json.IsOk)
        {
            File.WriteAllText("c:/temp/date.json",json.Value[0].content);
            var tree = JsonConvert.DeserializeObject<JObject>(json.Value[0].content);
            var token = tree.SelectToken("$.Children[0].Token");
            var dateTime = token.SelectToken("Value").Value<string>();
            if (dateTime == "2024.04.23")
            {
                Console.WriteLine("all is fine");
            }
            else
            {
                Console.WriteLine($"token is bad {dateTime} - expected 2024.04.23");
            }
        }
    }
}
}