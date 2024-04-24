using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace decompiler;

public class Decompiler
{
    public Decompiler()
    {
        
    }

    public string Decompile(Type lexerType, Type parserType)
    {
        StringBuilder builder = new StringBuilder();
        LexerDecompiler lexerDecompiler = new LexerDecompiler();
        var lexer = lexerDecompiler.DecompileLexer(lexerType);
        builder.AppendLine(lexer).AppendLine();
        ParserDecompiler parserDecompiler = new ParserDecompiler();
        var parser = parserDecompiler.DecompileParser(lexerType, parserType);
        builder.AppendLine(parser);
        return builder.ToString();
    }
    
    [ExcludeFromCodeCoverage]
    public string Decompile(string lexerFqn, string parserFqn, string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);
        var lexerType = assembly.GetType(lexerFqn);
        var parserType = assembly.GetType(parserFqn);
        return Decompile(lexerType, parserType);
    }
}