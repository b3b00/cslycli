using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.lexer;

namespace specificationExtractor;

public class LexerSpecificationExtractor
{
   

    private string Lexeme(string name, GenericToken type, params string[] args)
    {
        switch (type)
        {
            case GenericToken.Comment:
            {
                if (args.Length == 1)
                {
                    return $"[SingleLineComment] {name} : '{args[0].Replace("'","''")}';";
                }
                else if (args.Length == 2)
                {
                    return $"[MultiLineComment] {name} : '{args[0].Replace("'","''")}' '{args[1].Replace("'","''")}';";
                }

                break;
            }
            case GenericToken.Double:
            {
                return $"[Double] {name};";
            }
            case GenericToken.Identifier:
            {
                string attr = "AlphaId";
                if (args.Length == 1) {
                    if (Enum.TryParse<IdentifierType>(args[0], out var idType))
                    {
                        
                        switch (idType)
                        {
                            case IdentifierType.Alpha:
                            {
                                attr = "AlphaId";
                                break;
                            }
                            case IdentifierType.AlphaNumeric:
                            {
                                attr = "AlphaNumId";
                                break;
                            }
                            case IdentifierType.AlphaNumericDash:
                            {
                                attr = "AlphaNumDashId";
                                break;
                            }
                        }
                    }

                    
                }
                return $"[{attr}] {name};";
            }
            case GenericToken.Int:
            {
                return $"[Int] {name};";
            }
            case GenericToken.Char:
            {
                StringBuilder b = new StringBuilder();
                b.Append("[Character] ").Append(name);
                if (args.Length == 2)
                {
                    b.Append($" : '{args[0].Replace("'","''")}' '{args[1].Replace("'","''")}'");
                }

                b.Append(";");
                return b.ToString();
            }
            case GenericToken.String:
            {
                StringBuilder b = new StringBuilder();
                b.Append("[String] ").Append(name);
                if (args.Length == 2)
                {
                    b.Append($" : '{args[0].Replace("'","''")}' '{args[1].Replace("'","''")}'");
                }

                b.Append(";");
                return b.ToString();
            }
            case GenericToken.KeyWord:
            {
                return $"[KeyWord] {name} : '{args[0].Replace("'","''")}';";
            }
            case GenericToken.SugarToken:
            {
                return $"[Sugar] {name} : '{args[0].Replace("'","''")}';";
            }
            default:
            {
                return "";
            }


        }

        return "";
    }


    private string Lexeme(string name, string type, params string[] args)
    {
        if (type == "Sugar")
        {
            return Lexeme(name, GenericToken.SugarToken, args); 
        }

        else if (type == "Keyword")
        {
            return Lexeme(name, GenericToken.KeyWord, args);
        }

        else if (type == "Int")
        {
            return Lexeme(name, GenericToken.Int, args);
        }
        else if (type == "Double")
        {
            return Lexeme(name, GenericToken.Double, args);
        }
        else if (type == "SingleLineComment" || type == "MultiLineComment")
        {
            return Lexeme(name, GenericToken.Comment, args);
        }
        else if (type == "AlphaNumId")
        {
            return Lexeme(name, GenericToken.Identifier, IdentifierType.AlphaNumeric.ToString());
        }
        else if (type == "AlphaId")
        {
            return Lexeme(name, GenericToken.Identifier, IdentifierType.Alpha.ToString());
        }
        else if (type == "AlphaNumDashId")
        {
            return Lexeme(name, GenericToken.Identifier, IdentifierType.AlphaNumericDash.ToString());
        }
        else if (type == "String")
        {
            return Lexeme(name, GenericToken.String, args);
        }
        else if (type == "Character")
        {
            return Lexeme(name, GenericToken.Char, args);
        }
        else if (type == "Lexeme")
        {
            var t = args[0];
            args = args.Skip(1).ToArray();
            if (Enum.TryParse<GenericToken>(t, out var genericType))
            {
                return Lexeme(name, genericType, args);
            }
        }

        if (type == "Mode")
        {
            if (args.Any())
            {
                var t = string.Join(", ",args.Select(x => $"'{x}'"));
                return $@"[Mode({t})]";
            }

            return "[Mode]";
        }
        
        if (type == "Push")
        {
            if (args.Any())
            {
                var t = string.Join(", ",args.Select(x => $"'{x}'"));
                return $@"[Push({t})]";
            }

            return "[Push]";
        }

        return $"[{type}]";

    }


    
    public string ExtractFromFile(string lexerCsFileName)
    {
        var programText = File.ReadAllText(lexerCsFileName);
        return ExtractFromSource(programText);
    }
    
    public string ExtractFromSource(string lexerSource)
    {
        StringBuilder builder = new StringBuilder();
        
        var tree = CSharpSyntaxTree.ParseText(lexerSource);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        var lex =
            root.DescendantNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.EnumDeclaration)) as EnumDeclarationSyntax;
        
        if (lex != null)
        {
            builder.AppendLine($"genericLexer {lex.Identifier.Text};").AppendLine();
            foreach (var member in lex.Members)
            {
                if (member.AttributeLists.Any())
                {
                    var attributes = member.AttributeLists;
                    var modeAttributes = new List<string>() { "Mode", "Push,Pop" };
                    var modes = attributes.SelectMany(x => x.Attributes)
                        .Where(x => modeAttributes.Contains(x.Name.ToString()));
                    foreach (var attr in modes)
                    {
                        string[] pstrings = new string[] { };
                        if (attr?.ArgumentList?.Arguments != null && attr.ArgumentList.Arguments.Any())
                        {
                            pstrings = attr.ArgumentList.Arguments.Select(x => x.Expression.ExprToString()).ToArray();
                        }

                        var lexeme = Lexeme(member.Identifier.Text, attr.Name.ToString(), pstrings);
                        builder.AppendLine(lexeme);

                    }
                    
                    foreach (var attr in attributes.SelectMany(x => x.Attributes).Where(x => !modeAttributes.Contains(x.Name.ToString())))
                    {
                        string[] pstrings = new string[] { };
                        if (attr?.ArgumentList?.Arguments != null && attr.ArgumentList.Arguments.Any())
                        {
                            pstrings = attr.ArgumentList.Arguments.Select(x => x.Expression.ExprToString()).ToArray();
                        }

                        var lexeme = Lexeme(member.Identifier.Text, attr.Name.ToString(), pstrings);
                        builder.AppendLine(lexeme);

                    }
                }
            }
        }

        return builder.ToString();
    }
}