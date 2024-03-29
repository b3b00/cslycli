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
    
    public string GenerateLexer(LexerModel model, string nameSpace)
    {
        var head = GetHeader(model.Name, nameSpace);
        var body = GetBody(model);
        var foot = getFooter();

        var extensions = model.Tokens.Where(x => x.Type == GenericToken.Extension).ToList();
        string extender = "";
        if (extensions.Any())
        {
            extender = GetExtender(model);
        }
        var source = head+"\n"+body+"\n"+foot+"\n\n"+extender+"\n\n}";
        
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
        // source += "\n\nvar builder = lexer.FSMBuilder;\n\n";
        // source += "builder.GoTo(\"start\")\n";
        foreach (var chain in extension.Chains)
        {
            source += TransitionChain(chain,extension.Name) + "\n";
        }

        // source += $".CallBack(callback{extension.Name});";
        // source += "\n\n}";
        return source;
    }

    private string TransitionChain(TransitionChain chain, string extensionName)
    {
        var source = "";
        var tab = "            ";
        // source = $"if (token == {lexerModel.Name}.{extension.Name}) {{\n\n";
        // source += GetGenericCallBack(lexerModel.Name,extension.Name);
        source += "\n\nvar builder = lexer.FSMBuilder;\n\n";
        source += $"builder.GoTo(\"{chain.StartingNodeName}\")\n";
        foreach (var transition in chain.Transitions)
        {
            source += Transition(transition) + "\n";
        }

        if (chain.IsEnded)
        {
            source += $".End()";    
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
                    return $".TransitionTo('{charTransition.Character}',{toNode})";
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
                    return $".MultiRangeTransitionTo({toNode},{ranges})";
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
            var trans = Repeat(DoTransition, transition);
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
                    string loopingNode = Guid.NewGuid().ToString();
                    builder.AppendLine($".Mark({loopingNode})");
                    builder.AppendLine(doTransition(loopingNode));
                    break;
                }
                case RepeaterType.OneOrMore:
                {
                    string loopingNode = Guid.NewGuid().ToString();
                    builder.AppendLine(doTransition(null))
                        .AppendLine($".Mark({loopingNode})")
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
            builder.AppendLine(doTransition(null));
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
using sly.lexer.fsm;

namespace {nameSpace} {{

    public enum {name} {{
";
    }

    private string getFooter()
    {
        return @"
    }
";
    }
}