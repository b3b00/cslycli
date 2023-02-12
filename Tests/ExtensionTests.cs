using System.Collections.Generic;
using System.Reflection;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using csly.cli.parser;
using NFluent;
using SharpFileSystem.FileSystems;
using sly.lexer;
using Xunit;
using LexerBuilder = sly.lexer.LexerBuilder;

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
    public void TestExtLexerWithMarks()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/extWithMarks.txt");

        var lexbuild = LexerBuilder.BuildLexer<CLIToken>();
        Check.That(lexbuild.IsError).IsFalse();
        Check.That(lexbuild.Result).IsNotNull();
        var lexer = lexbuild.Result;
        var graph = (lexer as GenericLexer<CLIToken>).ToGraphViz();
        var lexed = lexer.Tokenize(grammar);
        Check.That(lexed.IsError).IsFalse();
        var tokens = lexed.Tokens.Tokens;
        Check.That(tokens).Not.IsNullOrEmpty();
        Check.That(tokens).CountIs(23);

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
    public void TestExtLexerWithMarks2()
    {
        var grammar = @"
genericLexer MinimalLexer;


[Extension] TEST
>>>
-> '#'  -> (loop) '|' -> (fin) '$' -> END
<<<



parser MinimalParser;

-> root : TEST;
";



        ParserContext context = new ParserContext("glop");

        var modelBuilder = new ParserBuilder();
        var model = modelBuilder.CompileModel(grammar, "strangeParser");
        Check.That(model).IsOkModel();
        
        var dot = modelBuilder.Getz(grammar, "#|$", "strangeParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot.IsError).IsFalse();
        Check.That(dot.Value).CountIs(1);
        var dotresult = dot.Value[0];
        Check.That(dotresult.format).Equals("DOT");
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var expected = fs.ReadAllText("/data/dotExtMarks.txt").Replace("\r\n","\n");
        
        Check.That(dotresult.content.Replace("\r\n","\n")).Equals(expected);

    }
    
    [Fact]
    public void TestExtLexerWithMarksAndChains()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/extWithMarksMultiChain.txt").Replace("\r\n","\n");
        
        ParserContext context = new ParserContext("glop");

        var modelBuilder = new ParserBuilder();
        var model = modelBuilder.CompileModel(grammar, "strangeParser");
        Check.That(model).IsOkModel();

        string test = "#*********#";
        var dot = modelBuilder.Getz(grammar, test, "strangeParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot.IsError).IsFalse();
        Check.That(dot.Value).CountIs(1);
        var dotresult = dot.Value[0];
        Check.That(dotresult.format).Equals("DOT");
        string expectation = $@"\""{test}\"""" shape=doublecircle height=0.50]";
        Check.That(dotresult.content.Replace("\r\n","\n")).Contains(expectation);
        
        test = "#******€";
        dot = modelBuilder.Getz(grammar, test, "strangeParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot.IsError).IsFalse();
        Check.That(dot.Value).CountIs(1);
        dotresult = dot.Value[0];
        Check.That(dotresult.format).Equals("DOT");
        expectation = $@"\""{test}\"""" shape=doublecircle height=0.50]";
        Check.That(dotresult.content.Replace("\r\n","\n")).Contains(expectation);

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