using System.Text;

namespace decompiler;

public class Decompiler
{
    public Decompiler()
    {
        
    }

    public string Decompile(string lexerFqn, string parserFqn, string assemblyPath)
    {
        StringBuilder builder = new StringBuilder();
        LexerDecompiler lexerDecompiler = new LexerDecompiler();
        var lexer = lexerDecompiler.DecompileLexer(
            assemblyPath,
            lexerFqn);
        builder.AppendLine(lexer).AppendLine();
        ParserDecompiler parserDecompiler = new ParserDecompiler();
        var parser = parserDecompiler.DecompileParser(
            assemblyPath,
            parserFqn, lexerFqn);
        builder.AppendLine(parser);
        return builder.ToString();
    }
}