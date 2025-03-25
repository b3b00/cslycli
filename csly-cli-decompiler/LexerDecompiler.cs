using System.Reflection;
using System.Text;
using sly.i18n;
using sly.lexer;

namespace decompiler;

public class LexerDecompiler
{

    public LexerDecompiler()
    {
        
    }

    public string DecompileLexer(Type lexerType)
    {
        var lex = GetLexer(lexerType);
        return lex;
    }

      private  string GetToken(string name, LexemeAttribute lexem)
        {
            
            switch (lexem.GenericToken)
            {
                case GenericToken.Extension:
                {
                    return $"[Extension] {name}; // extension builder will not be decompiled";
                }
                case GenericToken.Double:
                {
                    return $"[Double] {name};";
                }
                case GenericToken.Int:
                {
                    return $"[Int] {name};";
                }
                case GenericToken.Date:
                {
                    if (lexem.GenericTokenParameters.Length == 0)
                    {
                        return $"[Date] {name};";
                    }
                    else if (lexem.GenericTokenParameters.Length== 2)
                    {
                        return $"[Date] {name} : {lexem.GenericTokenParameters[0]} '{lexem.GenericTokenParameters[1]}' ;";
                    }
                    else if (lexem.GenericTokenParameters.Length == 1)
                    {
                        return $"[Date] {name} : {lexem.GenericTokenParameters[0]} ;";
                    }

                    break;
                }
                case GenericToken.Identifier:
                {
                    switch (lexem.IdentifierType)
                    {
                        case IdentifierType.Alpha:
                        {
                            return $"[AlphaId] {name};";
                        }
                        case IdentifierType.AlphaNumeric:
                        {
                            return $"[AlphaNumId] {name};";
                        }
                        case IdentifierType.AlphaNumericDash:
                        {
                            return $"[AlphaNumDashId] {name};";
                        }
                    }

                    break;
                }
                case GenericToken.Comment:
                {
                    if (lexem.GenericTokenParameters.Length == 1)
                    {
                        return $@"[SingleLineComment] {name} : ""{lexem.GenericTokenParameters[0].Replace("'","''")}"";";
                    }
                    else
                    {
                        return $@"[MultiLineComment] {name} : ""{lexem.GenericTokenParameters[0].Replace("'","''")} {lexem.GenericTokenParameters[1].Replace("'","''")}"";";
                    }
                }
                case GenericToken.Hexa:
                {
                    if (lexem.GenericTokenParameters.Length == 0)
                    {
                        return $@"[Hexa] {name} ;";
                    }
                    else
                    {
                        return $"[Hexa] {name} : \"{lexem.GenericTokenParameters[0].Replace("'","''")}\";";
                    }

                    break;
                }
                case GenericToken.KeyWord:
                {
                    return $@"[KeyWord] {name} : ""{lexem.GenericTokenParameters[0].Replace("'","''")}"";";
                }
                case GenericToken.UpTo:
                {
                    var delimiters = string.Join(" ",lexem.GenericTokenParameters.Select(x => $@"""{x}"""));
                    return $@"[UpTo] {name} : {delimiters};";
                }
                case GenericToken.SugarToken:
                {
                    return $@"[Sugar] {name} : ""{lexem.GenericTokenParameters[0].Replace("'","''")}"";";
                }
                case GenericToken.String:
                {
                    
                    if (lexem.GenericTokenParameters.Any())
                    {
                        var args =lexem.GenericTokenParameters.Take(2).ToList();
                        if (args[0] == "\"" && args[1] == "\\")
                        {
                            return $"[String] {name};";
                        }
                        return $"[String] {name} : {string.Join(" ", args)};";
                    }
                    return $"[String] {name};";
                }
                case GenericToken.Char:
                {
                    ;
                    if (lexem.GenericTokenParameters.Any())
                    {
                        var args =lexem.GenericTokenParameters.Take(2).ToList();
                        
                        return $"[Character] {name} : {string.Join(" ", args.Select(x => $"\"{(x.StartsWith("\\") || x.StartsWith("\"") ? "\\"+x : x)}\""))};";
                    }
                    return $"[Character] {name};";
                }
            }
            return $"[] {name}";
            
        }

        private string GetLexer(Type type)
        {

            if (type.IsEnum)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"genericLexer {type.Name};").AppendLine();
                var values = Enum.GetValues(type);

                var options =type.GetCustomAttributes<LexerAttribute>();
                if (options.Any())
                {
                    foreach (var lexerOptions in options)
                    {
                        builder.AppendLine($"[IgnoreEndOfLines({lexerOptions.IgnoreEOL.ToString().ToLower()})]");
                        builder.AppendLine($"[IgnoreWhiteSpaces({lexerOptions.IgnoreWS.ToString().ToLower()})]");
                        builder.AppendLine($"[IndentationAware({lexerOptions.IndentationAWare.ToString().ToLower()})]");
                        builder.AppendLine($"[IgnoreKeyWordCase({lexerOptions.KeyWordIgnoreCase.ToString().ToLower()})]");
                    }
                }

                foreach (Enum value in values)
                {
                    var modeAttributes = value.GetAttributesOfType<ModeAttribute>();
                    if (modeAttributes != null && modeAttributes.Any())
                    {
                        foreach (var modeAttribute in modeAttributes)
                        {
                            var mode = modeAttribute as ModeAttribute;
                            if (mode.Modes.Length >= 1 && !(mode.Modes.Length == 1 && mode.Modes[0] != ModeAttribute.DefaultLexerMode))
                            {
                                var modes = string.Join(", ",mode.Modes.Select(x => $@"""{x}"""));
                                builder.AppendLine($@"[Mode({modes})]");
                            }
                        }
                    }

                    var pushAttributes = value.GetAttributesOfType<PushAttribute>();
                    if (pushAttributes != null && pushAttributes.Any())
                    {
                        foreach (var pushAttribute in pushAttributes)
                        {
                            var push = pushAttribute as PushAttribute;
                            builder.AppendLine($@"[Push(""{push.TargetMode}"")]");
                        }
                    }
                    
                    var popAttributes = value.GetAttributesOfType<PopAttribute>();
                    if (popAttributes != null && popAttributes.Any())
                    {
                        foreach (var popAttribute in popAttributes)
                        {
                            builder.AppendLine($@"[Pop]");
                        }
                    }
                    
                    var labelAttributes = value.GetAttributesOfType<LexemeLabelAttribute>();
                    if (labelAttributes != null && labelAttributes.Any())
                    {
                        foreach (var labelAttribute in labelAttributes)
                        {
                            var label = labelAttribute as LexemeLabelAttribute;
                            builder.AppendLine($@"@label(""{label.Language}"",""{label.Label}"");");
                        }
                    }

                    if (value.ToString() == "CHAR")
                    {
                        ;
                    }
                    var lexemeAttributes = value.GetAttributesOfType<LexemeAttribute>();
                    if (lexemeAttributes!= null && lexemeAttributes.Any())
                    {
                        foreach (var attribute in lexemeAttributes)
                        {
                            var lexem = (attribute as LexemeAttribute);
                            builder.AppendLine(GetToken(value.ToString(), lexem));    
                        }
                        
                    }
                    
                    var commentAttributes = value.GetAttributesOfType<CommentAttribute>();
                    if (commentAttributes != null && commentAttributes.Any())
                    {
                        foreach (var commentAttribute in commentAttributes)
                        {
                            var comment = commentAttribute as CommentAttribute;
                            if (!string.IsNullOrEmpty(comment.SingleLineCommentStart))
                            {
                                builder.AppendLine($@"[SingleLineComment] {value.ToString()} : ""{comment.SingleLineCommentStart}"";");
                            }
                            else
                            {
                                builder.AppendLine($@"[MultiLineComment] {value.ToString()} : ""{comment.MultiLineCommentStart}"" ""{comment.MultiLineCommentEnd}"";");
                            }
                        }
                    }

                    builder.AppendLine();
                }

                return builder.ToString();
            }

            throw new Exception($"{type.FullName} is not an Enum");
        }
    
}