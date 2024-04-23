using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using decompiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NFluent;
using SharpFileSystem.FileSystems;
using sly.lexer;
using specificationExtractor;
using Xunit;

namespace Tests;

public class ExtractionTests
{
      [Fact]
    public void ReallyMetaTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        
        // compile CLI Parser from spec
        var grammar = fs.ReadAllText("/data/meta.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "GrammarParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        
        // generate CLI parser C# source code
        var parserGenerator = new ParserGenerator();
        var parserSource = parserGenerator.GenerateParser(model.Value, "grammar","object");
        Check.That(parserSource).IsNotNull();
        Check.That(parserSource).IsNotEmpty();
        var lexerGenerator = new LexerGenerator();
        // generate CLI lexer C# source code
        var lexerSource = lexerGenerator.GenerateLexer(model.Value.LexerModel, "grammar");
        Check.That(lexerSource).IsNotNull();
        Check.That(lexerSource).IsNotEmpty();

        // extract CLI parser spec from generated source code
        var extractor = new SpecificationExtractor();
        var specification = extractor.ExtractFromSource(lexerSource, parserSource);
        
        Check.That(specification).IsNotNull();
        Check.That(specification).IsNotEmpty();
        
        model = builder.CompileModel(specification, "GrammarParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();

    }
    
    
    
    [Fact]
    public void GenerateThenExtractSimpleExpressionTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/simpleExpression.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "SimpleExpressionParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
        var parserGenerator = new ParserGenerator();
        var parserSource = parserGenerator.GenerateParser(model.Value, "expression","int");
        Check.That(parserSource).IsNotNull();
        Check.That(parserSource).IsNotEmpty();
        var lexerGenerator = new LexerGenerator();
        var lexerSource = lexerGenerator.GenerateLexer(model.Value.LexerModel, "expression");
        Check.That(lexerSource).IsNotNull();
        Check.That(lexerSource).IsNotEmpty();

        var extractor = new SpecificationExtractor();
        var specification = extractor.ExtractFromSource(lexerSource, parserSource);
        
        Check.That(specification).IsNotNull();
        Check.That(specification).IsNotEmpty();
        
        model = builder.CompileModel(specification, "SimpleExpressionParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();

    }

    [Fact]
    public void TestSimpleExpressionExtraction()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var lexerSource = fs.ReadAllText("/data/simpleexpr/simpleexpressiontoken.csharp");
        var parserSource = fs.ReadAllText("/data/simpleexpr/simpleexpressionparser.csharp");


        var extractor = new SpecificationExtractor();
        var specification = extractor.ExtractFromSource(lexerSource, parserSource);
        
        Check.That(specification).IsNotNull();
        Check.That(specification).IsNotEmpty();
        
        var builder = new ParserBuilder();
        var model = builder.CompileModel(specification, "SimpleExpressionParser");
        Check.That(model.IsError).IsFalse();
        Check.That(model.Value).IsNotNull();
    }
    
    [Fact]
    public void TestManyLexemGrammarTest()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/manyLexemes.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "Many");
        Check.That(model.IsError).IsFalse();
        var errors = model.Error;
        
        var parseResult = builder.Getz(grammar, "if IF", "Many", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)});
        Check.That(parseResult.IsError).IsFalse();
        var r = parseResult.Value[0];
        Check.That(r.format).IsEqualTo("JSON");
        Check.That(r.content).IsNotNull().And.IsNotEmpty();
        var json = r.content;
        var o = JsonConvert.DeserializeObject<JObject>(json);
        var if1 = o.SelectToken(".Children[0].Children[0].Token.Value");
        Check.That(if1.Value<string>()).IsEqualTo("if");
        var if2 = o.SelectToken(".Children[0].Children[1].Token.Value");
        Check.That(if2.Value<string>()).IsEqualTo("IF");
    }
    
    
    [Fact]
    public void TestManyLexemGrammarCompileThenDecompileThenTest_ManyLexemes()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/manyLexemes.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "Many");
        Check.That(model).IsOk();
        var errors = model.Error;

        var p = builder.BuildParser(model);
        Check.That(p.lexerType).IsNotNull();
        Check.That(p.parserType).IsNotNull();

        var decompiler = new Decompiler();
        var decompiled = decompiler.Decompile(p.lexerType, p.parserType);
        Check.That(decompiled).IsNotNull().And.IsNotEmpty();
        builder.CompileModel(decompiled, "Many");
        
        var parseResult = builder.Getz(decompiled, "if IF", "Many", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)}, rootRule:"root");
        Check.That(parseResult.IsError).IsFalse();
        var r = parseResult.Value[0];
        Check.That(r.format).IsEqualTo("JSON");
        Check.That(r.content).IsNotNull().And.IsNotEmpty();
        var json = r.content;
        var o = JsonConvert.DeserializeObject<JObject>(json);
        var if1 = o.SelectToken(".Children[0].Children[0].Token.Value");
        Check.That(if1.Value<string>()).IsEqualTo("if");
        var if2 = o.SelectToken(".Children[0].Children[1].Token.Value");
        Check.That(if2.Value<string>()).IsEqualTo("IF");
    }
    
    [Fact]
    public void TestManyLexemGrammarCompileThenDecompileThenTest_Grammar1()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammar1.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MyParser1");
        Check.That(model).IsOkModel();
        var errors = model.Error;

        var p = builder.BuildParser(model);
        Check.That(p.lexerType).IsNotNull();
        Check.That(p.parserType).IsNotNull();

        var decompiler = new Decompiler();
        var decompiled = decompiler.Decompile(p.lexerType, p.parserType);
        Check.That(decompiled).IsNotNull().And.IsNotEmpty();
        builder.CompileModel(decompiled, "Many");
        
        var parseResult = builder.Getz(decompiled, "123 --", "MyParser1", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)}, rootRule:"expression");
        Check.That(parseResult.IsError).IsFalse();
        var r = parseResult.Value[0];
        Check.That(r.format).IsEqualTo("JSON");
        Check.That(r.content).IsNotNull().And.IsNotEmpty();
        var json = r.content;
        var o = JsonConvert.DeserializeObject<JObject>(json);
        var if1 = o.SelectTokens("$..Token.Value");
        var tokens = if1.ToList().Select(x => x.Value<string>()).ToList();
        Check.That(tokens).ContainsExactly(new List<string>() { "123", "--", "--" });
        ;
    }
    
    
    [Fact]
    public void TestManyLexemGrammarGeneration()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/manyLexemes.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "Many");
        Check.That(model.IsError).IsFalse();
        var errors = model.Error;

        var generator = new LexerGenerator();
        var generatedLexer = generator.GenerateLexer(model.Value.LexerModel, "many");

        Check.That(generatedLexer).IsNotNull().And.IsNotEmpty();

        
        var parseResult = builder.Getz(grammar, "if IF", "Many", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)});
        Check.That(parseResult.IsError).IsFalse();
        var r = parseResult.Value[0];
        Check.That(r.format).IsEqualTo("JSON");
        Check.That(r.content).IsNotNull().And.IsNotEmpty();
        var json = r.content;


    }
    
    [Fact]
    public void GenerateThenExtractDateTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = @"
genericLexer MinimalLexer;
[Date] DATE : YYYYMMDD '.';

parser MinimalParser;

-> root : DATE ;
";
        var builder = new ParserBuilder();
        var initialModel = builder.CompileModel(grammar, "DateParser");
        Check.That(initialModel.IsError).IsFalse();
        Check.That(initialModel.Value).IsNotNull();
        var parserGenerator = new ParserGenerator();
        var parserSource = parserGenerator.GenerateParser(initialModel.Value, "date","object");
        Check.That(parserSource).IsNotNull();
        Check.That(parserSource).IsNotEmpty();
        var lexerGenerator = new LexerGenerator();
        var lexerSource = lexerGenerator.GenerateLexer(initialModel.Value.LexerModel, "date");
        Check.That(lexerSource).IsNotNull();
        Check.That(lexerSource).IsNotEmpty();

        var extractor = new SpecificationExtractor();
        var specification = extractor.ExtractFromSource(lexerSource, parserSource);
        
        Check.That(specification).IsNotNull();
        Check.That(specification).IsNotEmpty();
        
        var extractedModel = builder.CompileModel(specification, "DateParser");
        Check.That(extractedModel.IsError).IsFalse();
        Check.That(extractedModel.Value).IsNotNull();

        Check.That(extractedModel.Value.LexerModel.Tokens).CountIs(initialModel.Value.LexerModel.Tokens.Count);
        var token0 = extractedModel.Value.LexerModel.Tokens[0];
        Check.That(token0.Type).IsEqualTo(GenericToken.Date);
        Check.That(token0.Args[0]).IsEqualTo(DateFormat.YYYYMMDD.ToString());
        Check.That(token0.Args[1]).IsEqualTo(".");
        Check.That(extractedModel.Value.ParserModel.Rules).CountIs(initialModel.Value.ParserModel.Rules.Count);
    }
    
}