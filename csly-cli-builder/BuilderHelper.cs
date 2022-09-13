namespace clsy.cli.builder;

public class BuilderHelper
{
    public static Type BuildGenericType(Type baseType, params Type[] typeParameters)
    {
        return baseType.MakeGenericType(typeParameters);
    }
}