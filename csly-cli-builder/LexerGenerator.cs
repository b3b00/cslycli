using System.Text;
using clsy.cli.builder.parser.cli.model;
using csly.cli.model.lexer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.lexer;

namespace clsy.cli.builder;

public class LexerGenerator
{

    public LexerGenerator()
    {
        
    }

    private int _markCounter = 0;
    
    public string GenerateLexer(LexerModel model, string nameSpace)
    {
        var head = GetHeader(model, nameSpace);
        var body = GetBody(model);
        var foot = getFooter();

        var extensions = model.Tokens.Where(x => x.Type == GenericToken.Extension).ToList();
        string extender = "";
        if (extensions.Any())
        {
            extender = GetExtender(model);
        }
        var source = head+"\n"+body+"}\n"+extender+"\n\n"+foot;
        
        var tree = CSharpSyntaxTree.ParseText(source);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
        var prettyPrintedSource = root .NormalizeWhitespace()
            .SyntaxTree
            .GetText(CancellationToken.None)
            .ToString();

        return prettyPrintedSource;


    }

    private string GetExtender(LexerModel model)
    {
        var head = GetExtenderHeader(model);
        var foot = GetExtenderFooter(model);
        var body = "";
        foreach (var extension in model.Tokens.Where(x => x.Type == GenericToken.Extension).Cast<ExtensionTokenModel>())
        {
            body += GetExtension(model, extension);
        }
        
        return head+"\n"+body+"\n"+foot;
    }

    private string GetExtenderHeader(LexerModel model)
    {
        return $@"    public class Extended{model.Name} {{

        public static void Extend{model.Name}({model.Name} token, LexemeAttribute lexem, GenericLexer<{model.Name}> lexer) {{
            ";  
    }
    
    private string GetExtenderFooter(LexerModel model)
    {
        return $@"
        }}    
    }}";  
    }
    
    #region extension

    private string GetGenericCallBack(string enumType, string tokenName)
    {
        return $@"NodeCallback<GenericToken> callback{tokenName} = (FSMMatch<GenericToken> match) => 
   	{{
        // this store the token id the the FSMMatch object to be later returned by GenericLexer.Tokenize 
        match.Properties[GenericLexer<{enumType}>.DerivedToken] = {enumType}.{tokenName};
        return match;
            }};";
    }
    
    private string GetExtension(LexerModel lexerModel, ExtensionTokenModel extension)
    {
        var source = "";
        var tab = "            ";
        source = $"if (token == {lexerModel.Name}.{extension.Name}) {{\n\n";
        source += GetGenericCallBack(lexerModel.Name,extension.Name);
        bool first = true;
        foreach (var chain in extension.Chains)
        {
            source += TransitionChain(chain,extension.Name,first) + "\n";
            first = false;
        }

        source += "\n}\n";
        return source;
    }

    private string TransitionChain(TransitionChain chain, string extensionName, bool first)
    {
        var source = "";
        var tab = "            ";
        // source = $"if (token == {lexerModel.Name}.{extension.Name}) {{\n\n";
        // source += GetGenericCallBack(lexerModel.Name,extension.Name);
        if (first)
        {
            source += "\n\nvar builder = lexer.FSMBuilder;\n\n";
        }
        source += $"builder.GoTo(\"{chain.StartingNodeName}\")\n";
        foreach (var transition in chain.Transitions)
        {
            source += Transition(transition) + "\n";
        }

        if (chain.IsEnded)
        {
            source += $".End(GenericToken.Extension)";    
            source += $".CallBack(callback{extensionName});";
        }
        else
        {
            source += ";\n";
        }

        return source;
    }

    #region transitions
    
    private string Transition(ITransition transition)
    {
        Func< string, string> DoTransition = null;
            
        if (transition is CharacterTransition charTransition)
        {
            DoTransition = (string toNode) =>
            {
                if (string.IsNullOrEmpty(toNode))
                {
                    return $".Transition('{charTransition.Character}')";
                }
                else
                {
                    return $@".TransitionTo('{charTransition.Character}',""{toNode}"")";
                }
                return "";
            };

        }
        else if (transition is RangeTransition rangeTransition)
        {
            DoTransition = (string toNode) =>
            {
                var ranges = string.Join(", ", rangeTransition.Ranges
                    .Select(x => ($"('{x.StartCharacter}', '{x.EndCharacter}')")));
                if (string.IsNullOrEmpty(toNode))
                {   
                    return $".MultiRangeTransition({ranges})";
                }
                else
                {
                    return $@".MultiRangeTransitionTo(""{toNode}"",{ranges})";
                }

                return "";
            };
        }
        else if (transition is ExceptTransition exceptTransition)
        {
            throw new NotImplementedException("except transitions are not yet implemented.");
        }

        if (DoTransition != null)
        {
            string trans = "";
            if (!string.IsNullOrEmpty(transition.Mark))
            {
                trans += $@".Mark(""{transition.Mark}"")";
            }
            trans += Repeat(DoTransition, transition);
            
            return trans;
        }

        

        return "";
    }

    
    private string Repeat(Func<string, string> doTransition, ITransition transition)
    {
        StringBuilder builder = new StringBuilder();
        string tab = "                    ";
        if (transition.Repeater != null)
        {
            switch (transition.Repeater.RepeaterType)
            {
                case RepeaterType.Count:
                {
                    for (int i = 0; i < transition.Repeater.Count; i++)
                    {
                        builder.AppendLine(doTransition(null));
                    }

                    break;
                }
                case RepeaterType.ZeroOrMore:
                {
                    _markCounter++;
                    string loopingNode = $"mark#{_markCounter}";
                    builder.AppendLine($@".Mark(""{loopingNode})""");
                    builder.AppendLine(doTransition(loopingNode));
                    break;
                }
                case RepeaterType.OneOrMore:
                {
                    _markCounter++;
                    string loopingNode = $"mark#{_markCounter}";
                    builder.AppendLine(doTransition(null))
                        .AppendLine($@".Mark(""{loopingNode}"")")
                        .AppendLine($"{doTransition(loopingNode)}");

                    break;
                }
                case RepeaterType.Option:
                {
                    throw new NotImplementedException("optional transitions are not yet implemented.");
                    break;
                }
            }
        }
        else
        {
            builder.AppendLine(doTransition(transition.Target));
        }
        return builder.ToString();
    }


    #endregion
    
   #endregion

    private string GetBody(LexerModel model)
    {
        
        Func<string, string> escapeChars = s =>
        {

            if (s == "\"")
            {
                return "\\\"";
            }

            if (s == "\\")
            {
                return "\\\\";
            }

            return s;
        };
        
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();
        foreach (var tokens in model.TokensByName)
        {
            foreach (var token in tokens.Value)
            {
                AddLabels(token, builder);

                if (token.IsPop)
                {
                    builder.AppendLine("\t\t[Pop]");   
                }

                if (token.IsPush)
                {
                    builder.AppendLine($@"      [Push(""{token.PushMode}"")]");
                }

                if (token.Modes.Count >= 1 && !(token.Modes.Count == 1 && token.Modes[0] == ModeAttribute.DefaultLexerMode))
                {
                    var modes = string.Join(", ",token.Modes.Select(x => $@"""{x}"""));
                    builder.AppendLine($@"      [Mode({modes})]");
                }
                
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
                    case GenericToken.Date:
                    {
                        string args = token.Args.Any()
                            ? string.Join(", ", token.Args.Select(x => $@"{escapeChars(x)}"))
                            : "";
                        if (token.Args.Length == 0)
                        {
                            builder.AppendLine("\t\t[Date]");
                        }
                        else if (token.Args.Length == 2)
                        {
                            builder.AppendLine($"\t\t[Date(DateFormat.{token.Args[0]},'{token.Args[1]}')]");
                        }
                        else if (args.Length == 1)
                        {
                            builder.AppendLine($"\t\t[Date(DateFormat.{token.Args[0]})]");
                        }
                        break;
                    }
                    case GenericToken.String:
                    {
                        string args = token.Args.Any() ? string.Join(", ", token.Args.Select(x => $@"""{escapeChars(x)}""")) : "";
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
                    case GenericToken.Char:
                    {
                        string args = token.Args.Any() ? string.Join(", ", token.Args.Select(x => $@"""{escapeChars(x)}""")) : "";
                        if (string.IsNullOrEmpty(args))
                        {
                            builder.AppendLine("\t\t[Character]");
                        }
                        else
                        {
                            builder.AppendLine($"\t\t[Character({args})]");
                        }

                        break;
                    }
                    case GenericToken.KeyWord:
                    {
                        builder.AppendLine($"\t\t[Keyword(\"{token.Args[0]}\")]");
                        break;
                    }
                    case GenericToken.UpTo:
                    {
                        var delimiters = string.Join(", ",token.Args.Select(x => $@"""{x}"""));
                        builder.AppendLine($"\t\t[UpTo({delimiters})]");
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

    private void AddLabels(TokenModel token, StringBuilder builder)
    {
        if (token.TryGetLabels(out var labels))
        {
            foreach (var label in labels)
            {
                builder.AppendLine($@"[LexemeLabel(""{label.Key}"",""{label.Value}"")]");
            }
        }
    }

    private string GetHeader(LexerModel model, string nameSpace)
    {
        StringBuilder b = new StringBuilder($@"
using sly.lexer;
using sly.lexer.fsm;
using sly.i18n;

namespace {nameSpace} {{");

        if (model.Options != null && model.Options.HasOptions)
        {
            b.Append("    [Lexer(");
            bool previous = false;
            if (model.Options.IgnoreWS.HasValue)
            {
                b.Append($"IgnoreWS={model.Options.IgnoreWS.Value.ToString().ToLower()}");
                previous = true;
            }

            if (model.Options.IndentationAware.HasValue)
            {
                if (previous)
                {
                    b.Append(", ");
                }

                b.Append($"IndentationAWare={model.Options.IndentationAware.Value.ToString().ToLower()}");
                previous = true;
            }

            if (model.Options.IgnoreEOL.HasValue)
            {
                if (previous)
                {
                    b.Append(", ");
                }

                b.Append($"IgnoreEOL={model.Options.IgnoreEOL.Value.ToString().ToLower()}");
                previous = true;
            }

            if (model.Options.IgnoreKeyWordCase.HasValue)
            {
                if (previous)
                {
                    b.Append(", ");
                }

                previous = true;
                b.Append($"KeyWordIgnoreCase={model.Options.IgnoreKeyWordCase.Value.ToString().ToLower()}");
            }

            b.AppendLine(")]");
        }

        b.AppendLine($@"    public enum {model.Name} {{");

        return b.ToString();
    }

    private string getFooter()
    {
        return @"    
}
";
    }
}