using System.Text;
using clsy.cli.builder.parser.cli.model;
using sly.lexer;

namespace clsy.cli.builder;

public class LexerGenerator
{
    public static string GenerateLexer(LexerModel model, string name, string nameSpace)
    {
        var head = GetHeader(name, nameSpace);
        var body = getBody(model);
        var foot = getFooter();
        return head+"\n"+body+"\n"+foot;
    }

    public static string getBody(LexerModel model)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();
        foreach (var token in model.Tokens)
        {
            switch (token.Type)
            {
                case GenericToken.Int:
                {
                    builder.AppendLine("\t\t[Int]");
                    break;
                } 
                case GenericToken.Double:
                {
                    builder.AppendLine("\t\t[Double]");
                    break;
                } 
                case GenericToken.String:
                {
                    //string args = token.Args.Any()? string.Join(", ",token.Args.Select(x => $@"""{x}""")) : "";
                    // if (string.IsNullOrEmpty(args))
                    // {
                        builder.AppendLine("\t\t[String]");
                    // }
                    // else
                    // {
                    //     builder.AppendLine($"\t\t[String({args})]");
                    // }
                    break;
                }
                case GenericToken.KeyWord:
                {
                    builder.AppendLine($"\t\t[Keyword(\"{token.Args[0]}\")]");
                    break;
                }
                case GenericToken.SugarToken:
                {
                    builder.AppendLine($"\t\t[Sugar(\"{token.Args[0]}\")]");
                    break;
                }
                case GenericToken.Identifier:
                {
                    builder.AppendLine($"\t\t[AlphaId]");
                    break;
                }
            }
            builder.AppendLine($"\t\t{token.Name},");
            builder.AppendLine();
        }


        return builder.ToString();
    }

    private static string GetHeader(string name, string nameSpace)
    {
        return $@"
using sly.lexer;

namespace {nameSpace} {{

    public enum {name} {{
";
    }

    private static string getFooter()
    {
        return @"
    }
}";
    }
}