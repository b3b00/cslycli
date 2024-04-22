using sly.lexer;

namespace decompiler;

public class ExtenderDecompiler
{
    public ExtenderDecompiler()
    {
        
    }

    public static Type BuildGenericType(Type baseType, params Type[] typeParameters)
    {
        return baseType.MakeGenericType(typeParameters);
    }
    
    public string Decompile(Type extenderType, Type lexerType)
    {
        var extensionMethods = extenderType.GetMethods().Where(x =>
        {
            var parameters = x.GetParameters();
            if (parameters.Length == 3)
            {
                if (parameters[0].ParameterType == lexerType)
                {
                    if (parameters[1].ParameterType == typeof(LexemeAttribute))
                    {
                        var genericLexerType = typeof(GenericLexer<>);
                        genericLexerType = BuildGenericType(genericLexerType, lexerType);
                        if (parameters[2].ParameterType == genericLexerType)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }).ToList();
        Console.WriteLine($"extensions = {string.Join(" ",extensionMethods.Select(x => x.Name))}");
        return null;
    }
}