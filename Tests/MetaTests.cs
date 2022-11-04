using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using NFluent;
using SharpFileSystem.FileSystems;
using Xunit;

namespace Tests;

public class MetaTests
{
   
    [Fact]
    public void MetaMetaTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/meta.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "GrammarParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var parserGenerator = new ParserGenerator();
        var source = parserGenerator.GenerateParser(model.Value, "grammar","object");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        var lexerGenerator = new LexerGenerator();
        source = lexerGenerator.GenerateLexer(model.Value.LexerModel, "grammar");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        var json = builder.Getz(grammar, grammar, "GrammarParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json.IsError).IsFalse();
        var content = json.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
    }
    
    [Fact]
    public void MetaWhileTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/meta.txt");
        var whileGrammar = fs.ReadAllText("/data/whileGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "GrammarParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var parserGenerator = new ParserGenerator();
        var source = parserGenerator.GenerateParser(model.Value, "grammar","object");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        var lexerGenerator = new LexerGenerator();
        source = lexerGenerator.GenerateLexer(model.Value.LexerModel, "grammar");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        var json = builder.Getz(grammar, whileGrammar, "GrammarParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json.IsError).IsFalse();
        var content = json.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
    }
    
    [Fact]
    public void MetaGrammar1Test()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/meta.txt");
        var grammar1 = fs.ReadAllText("/data/grammar1.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "GrammarParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var parserGenerator = new ParserGenerator();
        var source = parserGenerator.GenerateParser(model.Value, "grammar","object");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        var lexerGenerator = new LexerGenerator();
        source = lexerGenerator.GenerateLexer(model.Value.LexerModel, "grammar");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        var json = builder.Getz(grammar, grammar1, "GrammarParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json.IsError).IsFalse();
        var content = json.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
    }
}