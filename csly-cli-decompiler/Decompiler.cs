using System.Reflection;
using System.Text;

namespace decompiler;

public class Decompiler
{
    public Decompiler()
    {
        
    }

    public string Decompile(Type lexerType, Type parserType, Type extenderType = null)
    {
        StringBuilder builder = new StringBuilder();
        LexerDecompiler lexerDecompiler = new LexerDecompiler();
        var lexer = lexerDecompiler.DecompileLexer(lexerType);
        builder.AppendLine(lexer).AppendLine();
        ParserDecompiler parserDecompiler = new ParserDecompiler();
        var parser = parserDecompiler.DecompileParser(lexerType, parserType);
        builder.AppendLine(parser);
        if (extenderType != null)
        {
            ExtenderDecompiler extenderDecompiler = new ExtenderDecompiler();
            var x = extenderDecompiler.Decompile(extenderType, lexerType);
        }
        return builder.ToString();
    }
    
    public string Decompile(string lexerFqn, string parserFqn, string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            Console.Write(type.FullName);
        }
        var lexerType = assembly.GetType(lexerFqn);
        var parserType = assembly.GetType(parserFqn);
        var extenderType = assembly.GetType("my.name.space.ExtendedMinimalLexer");
        return Decompile(lexerType, parserType , extenderType);
    }
    
    
}