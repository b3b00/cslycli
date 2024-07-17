// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using clsy.cli.builder.parser;
using csly_cli_api;

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
        //Compile("C:\\Users\\olduh\\dev\\BlazorCslyViz\\BlazorVizView\\samples\\grammar\\indented-while.txt");
        // Extract(@"C:\Users\olduh\dev\csly\src\samples\IndentedWhile\parser\IndentedWhileParserGeneric.cs","C:\\Users\\olduh\\dev\\csly\\src\\samples\\IndentedWhile\\parser\\IndentedWhileTokenGeneric.cs", @"C:\Users\olduh\dev\BlazorCslyViz\BlazorVizView\samples\grammar\indented-while.txt");
        // Parse(@"C:\Users\olduh\dev\BlazorCslyViz\BlazorVizView\samples\grammar\indented-while.txt", @"C:\Users\olduh\dev\BlazorCslyViz\BlazorVizView\samples\source\indented-while.txt");
        TestErrorMessages();
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
            // File.WriteAllText("c:/temp/date.json",json.Value[0].content);
            //
            // var tree = JsonSerializer.Deserialize<JsonDocument>(json.Value[0].content);
            // //var tree = JsonConvert.DeserializeObject<JObject>(json.Value[0].content);
            // tree.RootElement.
            // var token = tree.SelectToken("$.Children[0].Token");
            // var dateTime = token.SelectToken("Value").Value<string>();
            // if (dateTime == "2024.04.23")
            // {
            //     Console.WriteLine("all is fine");
            // }
            // else
            // {
            //     Console.WriteLine($"token is bad {dateTime} - expected 2024.04.23");
            // }
        }
    }

    private static void Compile(string path)
    {
        var processor = new CslyProcessor();
        var r = processor.CompileModel(File.ReadAllText(path));
        if (r.IsOK)
        {
            Console.WriteLine("compilation ok");
        }
        else
        {
            foreach (var error in r.Errors)
            {
                Console.WriteLine(error);
            }
        }
            
    }

    private static void Extract(string parserPath, string lexrerPath, string outputPath = null)
    {
        var processor = new CslyProcessor();
        var e = processor.ExtractGrammar(File.ReadAllText(parserPath), File.ReadAllText(lexrerPath));
        if (e.IsOK)
        {
            if (outputPath != null)
            {
                File.WriteAllText(outputPath, e.Result);
            }

            Console.WriteLine("extraction ok");
            var r = processor.CompileModel(e.Result);
            if (r.IsOK)
            {
                Console.WriteLine("compilation ok");
            }
            else
            {
                foreach (var error in r.Errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
        else
        {
            foreach (var error in e.Errors)
            {
                Console.WriteLine(error);
            }
        }

    }
    
    private static void Parse(string grammarPath, string sourcePath, string outputPath = null)
        {
            var processor = new CslyProcessor();
            var e = processor.GetDot(File.ReadAllText(grammarPath), File.ReadAllText(sourcePath));
            if (e.IsOK)
            {
                if (outputPath != null)
                {
                    File.WriteAllText(outputPath, e.Result);
                }
    
                Console.WriteLine("parse ok");
                
            }
            else
            {
                foreach (var error in e.Errors)
                {
                    Console.WriteLine(error);
                }
            }
    
        }

    private static void TestErrorMessages()
    {
        string grammar = @"
genericLexer HexaLexer;

[Int] INT;

[Hexa] HEXA : ""6x"";
            
[AlphaId] ID;




parser HexaParser;


@node(""racine"");
-> root : hexa+ int;
@node(""hexadecimal"");
hexa : HEXA;
@node(""entier"");
int : INT;
";
        var processor = new CslyProcessor();
        var r = processor.CompileModel(grammar);
        if (r.IsOK)
        {
            Console.WriteLine("ok");
        }
        else
        {
            foreach (var error in r.Errors)
            {
                Console.WriteLine(error);
            }
        }

        var x = processor.GenerateParser(grammar,"hexa", "Object");
        if (x.IsOK)
        {
            File.WriteAllText("C:\\Users\\olduh\\dev\\csly-cli\\Generated\\SomeLexer.cs",x.Result.Lexer);
            File.WriteAllText("C:\\Users\\olduh\\dev\\csly-cli\\Generated\\SomeParser.cs",x.Result.Parser);
        }

        var t = processor.GetDot(grammar, "6x12 6xAB 6x28 999");
        if (t.IsOK)
        {
            File.WriteAllText("c:/temp/tree.dot",t.Result);
            Console.WriteLine("parse ok");
        }
        else
        {
            foreach (var error in t.Errors)
            {
                Console.WriteLine(error);
            }
        }
    }
}
}