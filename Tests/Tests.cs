using System;
using System.Collections.Generic;
using System.Diagnostics;
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

public class Tests
{
    [Fact]
    public void TestGrammar1()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammar1.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MyParser1");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var dot = builder.Getz(grammar, "2 + 2", "MyParser1", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot.IsError).IsFalse();

    }
    
    
    [Fact]
    public void TestGrammarWithImplicitsGenerator()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammarWithImplicits.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MyParser1");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var source = ParserGenerator.GenerateParser(model.Value, "ns","int");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();

    }
    
    [Fact]
    public void TestGrammarWithImplicits()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammarWithImplicits.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MyParser1");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var dot = builder.Getz(grammar, "2 + 2", "MyParser1", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot.IsError).IsFalse();

    }
    
    [Fact]
    public void TestWhileGrammar()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/whileGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "WhileParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var dot = builder.Getz(grammar, "(a:=0; while a < 10 do (print a; a := a +1 ))", "WhileParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot.IsError).IsFalse();
    }
    
    [Fact]
    public void TestWhileGrammarParserGenerator()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/whileGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "WhileParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var source = ParserGenerator.GenerateParser(model.Value, "ns","int");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
    }
    
    [Fact]
    public void TestBadGrammar()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/badGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MyParser1");
        Check.That(model.IsError).IsTrue();
        var errors = model.Error;
        Check.That(errors).CountIs(1);
        var error = errors[0];
        Check.That(error).Contains("unexpected end of stream");


    }
    
    [Fact]
    public void TestMinimalGrammar()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/minimalGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var json = builder.Getz(grammar, "2", "MinimalParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json.IsError).IsFalse();
        var content = json.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        var expected = fs.ReadAllText("/data/minimalJSON.json");
        Check.That(content).IsEqualTo(expected);
    }

    [Fact]
    public void GenerateLexerTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/minimalGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var source = LexerGenerator.GenerateLexer(model.Value.LexerModel, "ns");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        source = source.Replace("\r\n", "\n");
        var expected = fs.ReadAllText("/data/lexer.csharp").Replace("\r\n","\n");
        Check.That(source).IsEqualTo(expected);
        
  
    }
    
    [Fact]
    public void GenerateParserTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/minimalGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var source = ParserGenerator.GenerateParser(model.Value, "ns","int");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        source = source.Replace("\r\n", "\n");
        var expected = fs.ReadAllText("/data/parser.csharp").Replace("\r\n","\n");
        Check.That(source).IsEqualTo(expected);
    }

    [Fact]
    public void TestChoices()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammarX.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "grammarX");
        if (model.IsError)
        {
            model.Error.ForEach(Console.WriteLine);
        }
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var json = builder.Getz(grammar, "( * / - + ]", "grammarX", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        if (json.IsError)
        {
            json.Error.ForEach(x => Debug.WriteLine(x));
        }
        Check.That(json.IsError).IsFalse();
        var content = json.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }
    
    
    [Fact]
    public void TestGenerateChoices()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammarX.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "grammarX");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var source = ParserGenerator.GenerateParser(model.Value, "ns","object");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        Check.That(source)
            .Contains(
                "public object root_____LPAREN___RPAREN______PLUS___MINUS___TIMES___DIVIDE______LBRACK___RBRACK___(Token<MyLexer1> p0, List<Token<MyLexer1>> p1, List<Token<MyLexer1>> p2)");
    }
   
}