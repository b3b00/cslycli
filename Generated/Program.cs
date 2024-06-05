using sly.parser.generator;

namespace ns;

public class Program
{
    public static void Main(string[] args)
    {
        ParserBuilder<someLexer, int> builder = new ParserBuilder<someLexer, int>();
        var some = new someParser();
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
}