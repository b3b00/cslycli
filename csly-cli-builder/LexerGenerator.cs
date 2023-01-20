using System.Text;
using clsy.cli.builder.parser.cli.model;
using sly.lexer;

namespace clsy.cli.builder;

public class LexerGenerator
{

    public LexerGenerator()
    {
        
    }
    
    public string GenerateLexer(LexerModel model, string nameSpace)
    {
        var head = GetHeader(model.Name, nameSpace);
        var body = GetBody(model);
        var foot = getFooter();
        return head+"\n"+body+"\n"+foot;
    }

    private string GetBody(LexerModel model)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();
        foreach (var tokens in model.TokensByName)
        {
            foreach (var token in tokens.Value)
            {



                switch (token.Type)
                {
                    case GenericToken.Comment:
                    {
                        if (token.Args.Length == 1)
                        {
                            builder.AppendLine($@"      [SingleLineComment(""{token.Args[0]}"")]");
                        }
                        else if (token.Args.Length == 2)
                        {
                            builder.AppendLine($@"      [MultiLineComment(""{token.Args[0]}"",""{token.Args[1]}"")]");
                        }

                        break;
                    }
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
                        string args = token.Args.Any() ? string.Join(", ", token.Args.Select(x => $@"""{x}""")) : "";
                        if (string.IsNullOrEmpty(args))
                        {
                            builder.AppendLine("\t\t[String]");
                        }
                        else
                        {
                            builder.AppendLine($"\t\t[String({args})]");
                        }

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
                    case GenericToken.Extension:
                    {
                        builder.AppendLine($"\t\t[Extension]");
                        break;
                    }
                    case GenericToken.Identifier:
                    {
                        switch (token.IdentifierType)
                        {
                            case IdentifierType.Alpha:
                            {
                                builder.AppendLine($"\t\t[AlphaId]");
                                break;
                            }
                            case IdentifierType.AlphaNumeric:
                            {
                                builder.AppendLine($"\t\t[AlphaNumId]");
                                break;
                            }
                            case IdentifierType.AlphaNumericDash:
                            {
                                builder.AppendLine($"\t\t[AlphaNumDashId]");
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }

                        break;
                    }
                }
            }

            builder.AppendLine($"\t\t{tokens.Key},");
            builder.AppendLine();
        }


        return builder.ToString();
    }

    private string GetHeader(string name, string nameSpace)
    {
        return $@"
using sly.lexer;

namespace {nameSpace} {{

    public enum {name} {{
";
    }

    private string getFooter()
    {
        return @"
    }
}";
    }
}