﻿using System.Text;
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
            case GenericToken.Date:
            {
                return $"[Date] {name} : {args[0]} '{args[1]}';";
            }
            case GenericToken.Comment:
            {
                if (args.Length == 1)
                {
                    return $"[SingleLineComment] {name} : \"{args[0].Replace("\"","\\\"")}\";";
                }
                else if (args.Length == 2)
                {
                    return $"[MultiLineComment] {name} : \"{args[0].Replace("\"","\\\"")}\" \"{args[1].Replace("\"","\\\"")}\";";
                }

                break;
            }
            case GenericToken.Double:
            {
                return $"[Double] {name};";
            }
            case GenericToken.Hexa:
            {
                if (args.Length == 0)
                {
                    return $"[Hexa] {name};";
                }
                else if (args.Length == 1)
                {
                    return $"[Hexa] {name} : \"{args[0].Replace("\"","\\\"")}\";";
                }

                break;
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
                    b.Append($" : \"{args[0].Replace("\"","\\\"")}\" \"{args[1].Replace("\\","\\\\").Replace("\"","\\\"")}\"");
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
                    b.Append($" : \"{args[0].Replace("\"","\\\"")}\" \"{args[1].Replace("\\","\\\\").Replace("\"","\\\"")}\"");
                }

                b.Append(";");
                return b.ToString();
            }
            case GenericToken.KeyWord:
            {
                string definitions = string.Join(" ", args.Select(x => "\"" + x.Replace("\"", "\\\"") + "\""));
                
                return $"[KeyWord] {name} : {definitions};";
            }
            case GenericToken.SugarToken:
            {
                return $"[Sugar] {name} : \"{args[0].Replace("\"","\\\"")}\";";
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
        else if (type == "Hexa")
        {
            return Lexeme(name, GenericToken.Hexa, args);
        }
        else if (type == "Double")
        {
            return Lexeme(name, GenericToken.Double, args);
        }
        else if (type == "SingleLineComment" || type == "MultiLineComment")
        {
            return Lexeme(name, GenericToken.Comment, args);
        }
        else if (type == "Date")
        {
            return Lexeme(name, GenericToken.Date, args);
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
        else if (type == "UpTo")
        {
            return $@"[UpTo] {name} : {string.Join(" ",args.Select(x => $@"""{x}"""))}; ";
        }

        if (type == "Mode")
        {
            if (args.Any())
            {
                var t = string.Join(", ",args.Select(x => $@"""{x}"""));
                return $@"[Mode({t})]";
            }

            return "[Mode]";
        }
        
        if (type == "Push")
        {
            if (args.Any())
            {
                var t = string.Join(", ",args.Select(x => $@"""{x}"""));
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

            foreach (var attributes in lex.AttributeLists)
            {
                foreach (var attribute in attributes.Attributes)
                {
                    if (attribute.Name.ToString() == "Lexer")
                    {
                        var args = attribute.ArgumentList.Arguments;

                        if (args != null && args.Any())
                        {
                            foreach (var arg in args)
                            {
                                var value = arg.Expression;
                                var name = arg.NameEquals.Name.ToString();
                                if (name == "IgnoreEOL")
                                {
                                    builder.AppendLine($"[IgnoreEndOfLines({value.ToString()})]");
                                }

                                if (name == "IndentationAWare")
                                {
                                    builder.AppendLine($"[IndentationAware({value.ToString()})]");
                                }

                                if (name == "KeyWordIgnoreCase")
                                {
                                    builder.AppendLine($"[IgnoreKeyWordCase({value.ToString()})]");
                                }

                                if (name == "IgnoreWS")
                                {
                                    builder.AppendLine($"[IgnoreWhiteSpaces({value.ToString()})]");
                                }
                            }
                        }
                    }
                }
            }


            
            foreach (var member in lex.Members)
            {
                if (member.AttributeLists.Any())
                {
                    var attributes = member.AttributeLists;

                    var isExtension = attributes.Any(x => x.Attributes.Any(y => y.Name.ToString().Contains("Extension")));

                    if (isExtension) 
                    {
                        // we must ignore extension tokens as we do not know how to generate the extension form C# source
                        continue;
                    }
                    
                    var modeAttributes = new List<string>() { "Mode", "Push","Pop" };
                    
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
                        if (attr.Name.ToString().Contains("LexemeLabel"))
                        {
                            string[] lpstrings = new string[] { };
                            if (attr?.ArgumentList?.Arguments != null && attr.ArgumentList.Arguments.Any())
                            {
                                lpstrings = attr.ArgumentList.Arguments.Select(x => x.Expression.ExprToString())
                                    .ToArray();
                            }

                            var label = $@"@label(""{lpstrings[0]}"",""{lpstrings[1]}"");";
                            builder.AppendLine(label);
                        }
                        else
                        {
                            string[] pstrings = new string[] { };
                            if (attr?.ArgumentList?.Arguments != null && attr.ArgumentList.Arguments.Any())
                            {
                                Predicate<AttributeArgumentSyntax> filter = e =>
                                {
                                    if (e.NameColon != null && e.NameColon.Name.Identifier.Text == "channel")
                                    {
                                        return false;
                                    }

                                    if (e.NameColon != null && e.NameEquals.Name.Identifier.Text == "channel")
                                    {
                                        return false;
                                    }

                                    return true;
                                };
                                
                                pstrings = attr.ArgumentList.Arguments.Where(x => filter(x)).Select(x => x.Expression.ExprToString())
                                    .ToArray();
                            }

                            var lexeme = Lexeme(member.Identifier.Text, attr.Name.ToString(), pstrings);
                            builder.AppendLine(lexeme);
                        }
                    }

                    builder.AppendLine();
                }
            }
        }

        return builder.ToString();
    }
}