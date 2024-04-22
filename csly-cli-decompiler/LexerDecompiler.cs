using System.Reflection;
using System.Text;
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
                case GenericToken.KeyWord:
                {
                    return $@"[KeyWord] {name} : ""{lexem.GenericTokenParameters[0].Replace("'","''")}"";";
                }
                case GenericToken.SugarToken:
                {
                    return $@"[Sugar] {name} : ""{lexem.GenericTokenParameters[0].Replace("'","''")}"";";
                }
                case GenericToken.String:
                {
                    
                    if (lexem.GenericTokenParameters.Any())
                    {
                        var args =lexem.GenericTokenParameters.Select(x => $@"""{x.Replace("'", "''")}""").ToList();
                        return $"[String] {name} : {string.Join(" ", args)};";
                    }
                    return $"[String] {name};";
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

                foreach (Enum value in values)
                {
                    var attributes = value.GetAttributesOfType<LexemeAttribute>();
                    if (attributes.Any())
                    {
                        foreach (var attribute in attributes)
                        {
                            var lexem = (attribute as LexemeAttribute);
                            builder.AppendLine(GetToken(value.ToString(), lexem));    
                        }
                        
                    }
                }

                return builder.ToString();
            }

            throw new Exception($"{type.FullName} is not an Enum");
        }
    
}