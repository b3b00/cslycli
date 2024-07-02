using hexa;
using sly.parser.generator;

namespace ns;

public class Program
{
    public static void Main(string[] args)
    {
        //Some();
        Template();
    }

    private static void Some()
    {
        ParserBuilder<HexaLexer, int> builder = new ParserBuilder<HexaLexer, int>();
        var some = new HexaParser();
        var r = builder.BuildParser(some, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        if (r.IsOk)
        {
            var p =r.Result.Parse("1 + 2 3");
            if (p.IsError)
            {
                foreach (var error in p.Errors)
                {
                    Console.WriteLine(error);
                }
            }
            else
            {
                Console.WriteLine("parse is ok");
            }
                
        }
        else
        {
            foreach (var error in r.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
    
    private static void Template()
    {
        ParserBuilder<TemplateLexer, object> builder = new ParserBuilder<TemplateLexer, object>();
        var some = new TemplateParser();
        var r = builder.BuildParser(some, ParserType.EBNF_LL_RECURSIVE_DESCENT, "template") ?? throw new ArgumentNullException("builder.BuildParser(some, ParserType.EBNF_LL_RECURSIVE_DESCENT, \"template\")");
        if (r.IsOk)
        {
            var p =r.Result.Parse(@"hello-{=world=}-billy-{% if (a == 1) %}-bob-{%else%}-boubou-{%endif%}this is the end");
            if (p.IsError)
            {
                foreach (var error in p.Errors)
                {
                    Console.WriteLine(error);
                }
            }
            else
            {
                Console.WriteLine("parse is ok");
            }
                
        }
        else
        {
            foreach (var error in r.Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}