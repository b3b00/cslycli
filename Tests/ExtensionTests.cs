using System.Collections.Generic;
using System.Reflection;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using csly.cli.model;
using csly.cli.parser;
using NFluent;
using SharpFileSystem.FileSystems;
using sly.lexer;
using sly.parser.generator;
using Xunit;

namespace Tests;

public class ExtensionTests
{
    [Fact]
    public void TestExtLexer()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/ext.txt");

        var lexbuild = LexerBuilder.BuildLexer<CLIToken>();
        Check.That(lexbuild.IsError).IsFalse();
        Check.That(lexbuild.Result).IsNotNull();
        var lexer = lexbuild.Result;
        var graph = (lexer as GenericLexer<CLIToken>).ToGraphViz();
        var lexed = lexer.Tokenize(grammar);
        Check.That(lexed.IsError).IsFalse();
        var tokens = lexed.Tokens.Tokens;
        Check.That(tokens).Not.IsNullOrEmpty();
        Check.That(tokens).CountIs(24);

    }
    
    [Fact]
    public void TestExtLexer2()
    {
        var grammar = @"
genericLexer MinimalLexer;

[Extension] TEST
>>>
-> '#'  -> ['0'-'9','A'-'F'] {6} -> END
<<<

[Extension] AT
>>>
-> '@' -> END
<<<

parser MinimalParser;

-> root : TEST;
";



        ParserContext context = new ParserContext("glop");

        var modelBuilder = new ParserBuilder();
        var model = modelBuilder.CompileModel(grammar, "colorParser");
        Check.That(model).IsOkModel();
        
        var dot = modelBuilder.Getz(grammar, "#132456", "colorParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot.IsError).IsFalse();
        
    }
    
    [Fact]
    public void TestExtLexerSourceGen()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/minimalGrammarWithExt.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model).IsOkModel();
        var lexerGenerator = new LexerGenerator();
        var source = lexerGenerator.GenerateLexer(model.Value.LexerModel, "ns");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        source = source.Replace("\r\n", "\n");
        var expected = fs.ReadAllText("/data/lexerWithExt.csharp").Replace("\r\n","\n");
        Check.That(source).IsEqualTo(expected);
    }
    
    
      
}