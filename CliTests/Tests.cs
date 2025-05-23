using System.Globalization;
using System.Reflection;
using System.Text.Json;
using clsy.cli.builder;
using clsy.cli.builder.parser;
using csly_cli_api;
using csly.cli.model.parser;
using csly.cli.model.tree;
using csly.cli.parser;
using decompiler;
using NFluent;
using SharpFileSystem.FileSystems;
using sly.buildresult;
using sly.lexer;
using Xunit;

namespace CliTests;

public class Tests
{
    private CslyProcessor _processor;

    public Tests()
    {
        _processor = new CslyProcessor();
    }
    
    [Fact]
    public void TestGrammar1()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammar1.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MyParser1");
        
        Check.That(model).IsOkModel();
        var dot = builder.Getz(grammar, "2 + 2", "MyParser1", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot).IsOkResult();

    }
    
    [Fact]
    public void TestCsproj()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/xmlGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "XmlParser");

        var source = fs.ReadAllText("/data/csproj.project");
        
        Check.That(model).IsOkModel();
        var dot = builder.Getz(grammar, source, "XmlParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot).IsNotOkResult();
        var errors = dot.Error;
        Check.That(errors[0]).Contains("unexpected end of stream");
        

    }
    
    
    [Fact]
    public void TestSubNodeNames()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/xmlGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "XmlParser");
        Check.That(model).IsOkModel();
        var source = @"
<xml>
<node>value1</node>
<othernode>value2</othernode>
<!-- comment -->
data
</xml>";
        var dot = builder.Getz(grammar, source, "XmlParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot).IsOkResult();
        Check.That(dot.Value[0].content).Contains("label=\"elements\"")
            .And.Not.Contains("label=\"null\"");


    }
    
    [Fact]
    public void TestGenerateWithSubNodeNames()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/xmlGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "XmlParser");
        Check.That(model).IsOkModel();
        var generated = _processor.GenerateParser(grammar,"xml","string");
        Check.That(generated.Result.Parser).Contains("[SubNodeNames(null, \"elements\", null)]")
        ;


    }
    
    [Fact]
    public void TestGenerateThenExtractWithSubNodeNames()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/xmlGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "XmlParser");
        Check.That(model).IsOkModel();
        var generated = _processor.GenerateParser(grammar,"xml","string");
        Check.That(generated.Result.Parser).Contains("[SubNodeNames(null, \"elements\", null)]");
        var extracted =_processor.ExtractGrammar(generated.Result.Parser, generated.Result.Lexer);
        Check.That(extracted).IsOkCliResult();
        var spec = extracted.Result;
        Check.That(spec).IsNotNull().And.IsNotEmpty();
        Check.That(spec).Contains("@subNodes(null, elements, null);");
        var compiledExtraction = _processor.CompileModel(spec);
        Check.That(compiledExtraction).IsOkCliResult();
        var parserModel = compiledExtraction.Result.ParserModel;
        Check.That(parserModel).IsNotNull();
        var checkedRule = parserModel.Rules.FirstOrDefault(x => x.Attributes.Any(y => y.Key == "subNodes"));
        Check.That(checkedRule).IsNotNull();
        var attributes = checkedRule.Attributes.FirstOrDefault(x => x.Key == "subNodes").Value;
        Check.That(attributes).IsNotNull().And.IsSingle();
        var parameters = attributes[0].AttributeValues;
        Check.That(parameters).IsEqualTo(new []{"null","elements","null"});
    
    }

    [Fact]
    public void TestCompileThenDecompileXml()
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/xmlGrammar.txt");
        
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalXmlParser");
        Check.That(model).IsOkModel();
        var errors = model.Error;

        var p = builder.BuildParser(model);
        Check.That(p.lexerType).IsNotNull();
        Check.That(p.parserType).IsNotNull();

        var decompiler = new Decompiler();
        var decompiled = decompiler.Decompile(p.lexerType, p.parserType);
        Check.That(decompiled).IsNotNull().And.IsNotEmpty();
        var rc = builder.CompileModel(decompiled, "MinimalXmlParser");
        Check.That(rc).IsOkModel();
        
        var parserModel = rc.Value.ParserModel;
        Check.That(parserModel).IsNotNull();
        var checkedRule = parserModel.Rules.FirstOrDefault(x => x.Attributes.Any(y => y.Key == "subNodes"));
        Check.That(checkedRule).IsNotNull();
        var attributes = checkedRule.Attributes.FirstOrDefault(x => x.Key == "subNodes").Value;
        Check.That(attributes).IsNotNull().And.IsSingle();
        var parameters = attributes[0].AttributeValues;
        Check.That(parameters).IsEqualTo(new []{"null","elements","null"});
        
    }
    
    [Fact]
    public void TestGrammarWithImplicitsGenerator()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammarWithImplicits.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MyParser1");
        Check.That(model).IsOkModel();
        var parserGenerator = new ParserGenerator();
        var source = parserGenerator.GenerateParser(model.Value, "ns","int");
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
        Check.That(model).IsOkModel();
        var dot = builder.Getz(grammar, "2 + 2", "MyParser1", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot).IsOkResult();

    }
    
    [Fact]
    public void TestWhileGrammar()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/whileGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "WhileParser");
        Check.That(model).IsOkModel();
        
        var dot = builder.Getz(grammar, "(a:=0; while a < 10 do (print a; a := a +1 ))", "WhileParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot).IsOkResult();
    }
    
    [Fact]
    public void TestWhileGrammarParserGenerator()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/whileGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "WhileParser");
        Check.That(model).IsOkModel();
        var parserGenerator = new ParserGenerator();
        var source = parserGenerator.GenerateParser(model.Value, "ns","int");
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
        Check.That(model).IsNotOkModel();
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
        Check.That(model).IsOkModel();
        Check.That(model.Value).IsNotNull();
        var json = builder.Getz(grammar, "2", "MinimalParser", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json).IsOkResult();
        var content = json.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        var expected = fs.ReadAllText("/data/minimalJSON.json");
        Check.That(content).IsEqualToJson(expected);
        
    }

    [Fact]
    public void GenerateLexerTest()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/minimalGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model).IsOkModel();
        var lexerGenerator = new LexerGenerator();
        var source = lexerGenerator.GenerateLexer(model.Value.LexerModel, "ns");
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
        Check.That(model).IsOkResult();
        Check.That(model.Value).IsNotNull();
        var parserGenerator = new ParserGenerator();
        var source = parserGenerator.GenerateParser(model.Value, "ns","int");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        source = source.Replace("\r\n", "\n");
        var expected = fs.ReadAllText("/data/parser.csharp").Replace("\r\n","\n");
        Check.That(source).IsEqualTo(expected);
    }

    [Fact]
    public void TestOption()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/optionGrammar.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "P");
        Check.That(model).IsOkModel();
        var dot = builder.Getz(grammar, "a b", "grammarX", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot).IsOkResult();
        var content = dot.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
        dot = builder.Getz(grammar, "a ", "grammarX", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToDotGraph)});
        Check.That(dot).IsOkResult();
        content = dot.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains("ε", content);
    }
    
    [Fact]
    public void TestChoices()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/grammarX.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "grammarX");
        Check.That(model).IsOkModel();
        var json = builder.Getz(grammar, "( * / - + ]", "grammarX", new List<(string format, SyntaxTreeProcessor processor)>() {("DOT",ParserBuilder.SyntaxTreeToJson)});
        
        Check.That(json).IsOkResult();
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
        Check.That(model).IsOkModel();
        var parserGenerator = new ParserGenerator();
        var source = parserGenerator.GenerateParser(model.Value, "ns","object");
        Check.That(source).IsNotNull();
        Check.That(source).IsNotEmpty();
        Check.That(source)
            .Contains(
                "public object root_LPAREN_RPAREN_PLUS_MINUS_TIMES_DIVIDE_LBRACK_RBRACK_(Token<MyLexer1> p0, List<Token<MyLexer1>> p1, List<Token<MyLexer1>> p2)");
    }

    [Fact]
    public void TestParserOptimizations()
    {
        var grammar = @"
genericLexer MinimalLexer;
[Int] INT;

parser MinimalParser;
[UseMemoization]
[BroadenTokenWindow]
-> root : INT ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model).IsOkModel();
        Check.That(model.Value.ParserModel.UseMemoization).IsTrue();
        Check.That(model.Value.ParserModel.BroadenTokenWindow).IsTrue();
    }
    
    [Fact]
    public void TestDateToken()
    {
        var grammar = @"
genericLexer MinimalLexer;
[Date] DATE : YYYYMMDD '.';

parser MinimalParser;

-> root : DATE ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model).IsOkModel();
        Check.That(model.Value.LexerModel.Tokens).CountIs(1);
        var token = model.Value.LexerModel.Tokens[0];
        Check.That(token.Name).IsEqualTo("DATE");
        Check.That(token.Type).IsEqualTo(GenericToken.Date);
        Check.That(token.Args).CountIs(2);
        Check.That(token.Args[0]).IsEqualTo("YYYYMMDD");
        Check.That(token.Args[1]).IsEqualTo(".");

        var generator = new LexerGenerator();
        var lexer = generator.GenerateLexer(model.Value.LexerModel, "namespace");
        ;
        var json = builder.Getz(grammar, "2024.04.23", "MyDateParser", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json).IsOkResult();
        var tree = JsonSerializer.Deserialize<JsonDocument>(json.Value[0].content);
        JsonElement firstToken = tree.RootElement.GetProperty("Children")[0].GetProperty("Token");
        var dateTime = firstToken.GetProperty("Value").GetString();
        Check.That(dateTime).IsEqualTo("2024.04.23");
    }
    
    [Fact]
    public void TestLexerOptions()
    {
        var grammar = @"
genericLexer MinimalLexer;
[IndentationAware(false)]
[IgnoreKeyWordCase(true)]

[AlphaNumDashId] ID;
[KeyWord] HELLO : ""hello"";
[KeyWord] WORLD : ""world"";

parser MinimalParser;

-> root : HELLO WORLD ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MinimalParser");
        Check.That(model).IsOkModel();
        Check.That(model.Value.LexerModel.Options.IgnoreKeyWordCase.Value).IsTrue();
        Check.That(model.Value.LexerModel.Options.IndentationAware.Value).IsFalse();
        Check.That(model.Value.LexerModel.Options.IgnoreWS).IsNull();
        Check.That(model.Value.LexerModel.Options.IgnoreEOL).IsNull();

        var generator = new LexerGenerator();
        var lexer = generator.GenerateLexer(model.Value.LexerModel, "namespace");
        ;
        var json = builder.Getz(grammar, "hello world", "MyParser", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json).IsOkResult();
        json = builder.Getz(grammar, "HELLO woRld", "MyParser", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json).IsOkResult();
        
    }
    
    
    [Fact]
    public void TestExpressions()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/expr.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "ExprParser");
        Check.That(model).IsOkModel();
        Check.That(model.Value).IsNotNull();
        var json = builder.Getz(grammar, "1 / 2 / 3 + 4", "ExprParser", new List<(string format, SyntaxTreeProcessor processor)>() {("JSON",ParserBuilder.SyntaxTreeToJson)});
        Check.That(json).IsOkResult();
        var content = json.Value.First().content;
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }
    
    [Fact]
    public void TestNoRootGrammar()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/noRoot.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "NoRootParser");
        Check.That(model).IsNotOkModel();
        Check.That(model.Error).Contains("model have no root rule !");
    }
    
    [Fact]
    public void TestMissingReference()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/missingReference.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "MissingReferenceParser");
        Check.That(model).IsNotOkModel();
        Check.That(model.Error).CountIs(4);
    }
    
    [Fact]
    public void TestLeftRecursion()
    {
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/leftRecursive.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "LeftRecursiveParser");
        Check.That(model).IsNotOkModel();
        Check.That(model.Error).CountIs(2);
    }

    [Fact]
    public void TestSameKeywordDefinition()
    {
        var grammar = @"
genericLexer SameKeywordLexer;

[AlphaNumDashId] ID;
[KeyWord] HELLO : ""hello"";
[KeyWord] HI : ""hello"";
[KeyWord] WORLD : ""world"";


parser SameKeywordParser;

-> root : [HELLO|HI] WORLD ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "SameKeywordParser");
        Check.That(model).IsNotOkModel();
        Check.That(model.Error).CountIs(1);
        Check.That(model.Error[0]).Contains("hello");
        Check.That(model.Error[0]).Contains("HELLO");
        Check.That(model.Error[0]).Contains("HI");
    }
    
    [Fact]
    public void TestSameSugarDefinition()
    {
        var grammar = @"
genericLexer SameKeywordLexer;

[Sugar] DOT : ""."";
[Sugar] DOOT : ""."";

parser SameKeywordParser;

-> root : DOT* ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "SameKeywordParser");
        Check.That(model).IsNotOkModel();
        Check.That(model.Error).CountIs(1);
        Check.That(model.Error[0]).Contains(".");
        Check.That(model.Error[0]).Contains("DOT");
        Check.That(model.Error[0]).Contains("DOOT");
    }
    
    [Fact]
    public void TestNotEndedExtensionToken()
    {
        var grammar = @"
genericLexer NotEndedExtensionLexer;

[Extension] TEST
>>>
-> '#'  -> ['0'-'9','A'-'F'] {6}
<<<


parser NotEndedExtensionParser;

-> root : TEST* ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "NotEndedExtensionParser");
        Check.That(model.IsOk).IsFalse();
        Check.That(model.Error).CountIs(1);
        Check.That(model.Error[0]).Contains("TEST");
    }

    [Fact]
    public void ManyTokenForOperations()
    {
        var grammar = @"
genericLexer someLexer;

[Int] INT;
[Sugar] PLUS : ""+""; 
[KeyWord] ADD:""and"";
[Sugar] MINUS : ""-"";
[KeyWord] REMOVE:""remove"";
[Sugar] TIMES : ""*"";
[KeyWord] MUL:""mul"";
[Sugar] SLASH : ""/"";
[KeyWord] DIV:""div"";



parser someParser;

-> root : someParser_expressions;

[Right 10] PLUS ADD;
@name(minus);
[Right 10] MINUS REMOVE;

[Right 50] TIMES MUL;
@name(div);
[Right 50] DIV SLASH;

#@name(prefixPlus);
[Prefix 100] PLUS ADD ""#"";

[Prefix 100] MINUS REMOVE ""~"";


[Operand] 
integer : INT;

";

        string source = @"
1 / 2 / 3 + 4
";

        var r = _processor.GetDot(grammar," 0 + ~1 + -2 div #3 ");
        Check.That(r.IsOK).IsTrue();

    }

    [Fact]
    public void ExplicitOperationToken()
    {
        var grammar = @"
genericLexer explicitL;
[Int] INT;

parser explicitP;
-> root : explicitP_expressions;
[Right 10] ""+"";
[Prefix 100] ""##"";
[Postfix 110] ""??"";
[Operand] value : INT;" ;
        var t = _processor.CompileModel(grammar);
        Check.That(t.IsOK).IsTrue();
        var r = _processor.GetSyntaxTree(grammar, "##2 + 2??");
        Check.That(r).IsNotNull();
        Check.That(r.IsOK).IsTrue();
        Check.That(r.Result).IsNotNull();
        Check.That(r.Result).IsInstanceOf<SyntaxNode>();
    }
    
    [Fact]
    public void ExplicitOperationTokenSyntaxNodeNames()
    {
        var grammar = @"
genericLexer explicitL;
[Int] INT;

parser explicitP;
-> root : explicitP_expressions;
[Prefix 100] ""##"";
[Operand] value : INT;" ;
        var t = _processor.CompileModel(grammar);
        Check.That(t.IsOK).IsTrue();
        var r = _processor.GetSyntaxTree(grammar, "##2");
        Check.That(r).IsNotNull();
        Check.That(r.IsOK).IsTrue();
        Check.That(r.Result).IsNotNull();
        Check.That(r.Result).IsInstanceOf<SyntaxNode>();
        var root = r.Result as SyntaxNode;
        var c1 = root.Children[0];
        Check.That(c1).IsNotNull();
        Check.That(c1).IsInstanceOf<SyntaxNode>();
        Check.That((c1 as SyntaxNode).IsByPassNode).IsTrue();
        var c2 = (c1 as SyntaxNode).Children[0];
        Check.That(c2).IsNotNull();
        Check.That(c2).IsInstanceOf<SyntaxNode>();
        Check.That(c2.Name).Not.IsNullOrWhiteSpace();
    }
    
    [Fact]
    public void ApiParse()
    {
        var grammar = @"
genericLexer MinimalLexer;
[Date] DATE : YYYYMMDD '.';

parser MinimalParser;

-> root : DATE ;
";
        
        var r = _processor.Parse(grammar, "2024.04.23");
        Check.That(r.IsOK).IsTrue();

        r = _processor.Parse(grammar, "coucou");
        Check.That(r.IsOK).IsFalse();
        
        r = _processor.Parse(grammar, "2024.06.05.13.21");
        Check.That(r.IsOK).IsFalse();
    }
    
    [Fact]
    public void Upto()
    {
        var grammar = @"
genericLexer TemplateLexer;

[UpTo] TEXT : ""{%"" ""{="";

[Push(""code"")]
[Sugar] OPEN_CODE : ""{%"";

[Push(""value"")]
[Sugar] OPEN_VALUE : ""{="";

[Mode(""value"", ""code"")]
[AlphaId] ID;

[Mode(""code"")]
[Int] INT;

[Mode(""code"")]
[String] STRING;

[Mode(""value"")]
[Pop]
[Sugar] CLOSE_VALUE : ""=}"";

[Mode(""code"")]
[Pop]
[Sugar] CLOSE_CODE : ""%}"";



parser TemplateParser;

-> template : item *;
item : TEXT;
item : OPEN_VALUE ID CLOSE_VALUE;
item : OPEN_CODE ID CLOSE_CODE;
item : OPEN_CODE INT CLOSE_CODE;
item : OPEN_CODE STRING CLOSE_CODE;
";

        var r = _processor.Parse(grammar, @"text{=value=}text{%28%}text{%""hello""%}text");
        Check.That(r.IsOK).IsTrue();

        r = _processor.Parse(grammar, "text{=value");
        Check.That(r.IsOK).IsFalse();
        
    }

    [Fact]
    public void TestMissingOperand()
    {
        string grammar = @"
genericLexer l;
[Int] INT;
parser p;
-> root: p_expressions;
[Prefix 10] ""++"";
";
        _processor.CompileModel(grammar);
        var parseResult = _processor.GetSyntaxTree(grammar, "++2");
        Check.That(parseResult).IsNotOkCliResult();
        Check.That(parseResult.Errors).IsSingle();
        var error = parseResult.Errors[0];
        Check.That(error).Contains(ErrorCodes.PARSER_MISSING_OPERAND.ToString());
        var compilationResult = _processor.Compile(grammar);
        Check.That(compilationResult).IsNotOkCliResult();
        Check.That(compilationResult.Errors).IsSingle();
        error = compilationResult.Errors[0];
        Check.That(error).Contains(ErrorCodes.PARSER_MISSING_OPERAND.ToString());

    }

    [Fact]
    public void TestShortOperandSingle()
    {
        string grammar = @"
genericLexer l;
[Int] INT;
parser p;
-> root: p_expressions;
[Prefix 10] ""++"";
[Operand] INT;
";
        var model = _processor.CompileModel(grammar);
        Check.That(model.IsOK).IsTrue();
        var operand = model.Result.ParserModel.Rules.FirstOrDefault(x => x.IsOperand);
        Check.That(operand).IsNotNull();
        Check.That(operand.Clauses).IsSingle();
        Check.That(operand.Clauses[0]).IsInstanceOf<TerminalClause>();
        Check.That((operand.Clauses[0] as TerminalClause).TokenName).IsEqualTo("INT");
        var parseResult = _processor.GetSyntaxTree(grammar, "++2");
        Check.That(parseResult.IsOK).IsTrue();

    }
    
    [Fact]
    public void TestShortOperandMany()
    {
        string grammar = @"
genericLexer l;
[Int] INT;
[Double] DOUBLE;
parser p;
-> root: p_expressions;
[Right 10] ""+"";
[Operand] INT DOUBLE;
";
        var model = _processor.CompileModel(grammar);
        Check.That(model.IsOK).IsTrue();
        var operand = model.Result.ParserModel.Rules.FirstOrDefault(x => x.IsOperand);
        Check.That(operand).IsNotNull();
        Check.That(operand.Clauses).IsSingle();
        Check.That(operand.Clauses[0]).IsInstanceOf<ChoiceClause>();
        Check.That((operand.Clauses[0] as ChoiceClause).Choices).CountIs(2);
        var parseResult = _processor.GetSyntaxTree(grammar, "1 + 2.3");
        Check.That(parseResult.IsOK).IsTrue();

    }
 
    [Fact]
    public void TestShortOperandManyWithMixedChoicesError()
    {
        string grammar = @"
genericLexer l;
[Int] INT;
[Double] DOUBLE;
parser p;
-> root: p_expressions;
[Right 10] ""+"";
group : ""("" p_expressions "")""; 
[Operand] INT DOUBLE group;
";
        var model = _processor.Compile(grammar);
        Check.That(model.IsOK).IsFalse();
        Check.That(model.Errors).IsSingle();
        Check.That(model.Errors[0]).Contains("[PARSER_MIXED_CHOICES]");
    }
    
    [Fact]
    public void TestShortOperandManyWithMixedChoicesOk()
    {
        string grammar = @"
genericLexer l;
[Int] INT;
[Double] DOUBLE;
parser p;
-> root: p_expressions;
[Right 10] ""+"";
group : ""("" p_expressions "")""; 
[Operand] INT DOUBLE;
[Operand] group;
";
        var model = _processor.CompileModel(grammar);
        Check.That(model.IsOK).IsTrue();
        var operand = model.Result.ParserModel.Rules.FirstOrDefault(x => x.IsOperand);
        Check.That(operand).IsNotNull();
        Check.That(operand.Clauses).IsSingle();
        Check.That(operand.Clauses[0]).IsInstanceOf<ChoiceClause>();
        Check.That((operand.Clauses[0] as ChoiceClause).Choices).CountIs(2);
        var parseResult = _processor.GetSyntaxTree(grammar, "1 + 2.3");
        Check.That(parseResult.IsOK).IsTrue();
    }

    [Fact]
    public void TestOperationNames()
    {
        string grammar = @"
genericLexer l;
[Int] INT;
parser p;
@name(racine);
-> root: p_expressions;
@name(pupuce);
@node(pupuce);
[Right 10] ""+"";
@name(mimi);
@node(mimi);
[Right 10] ""-"";
@node(post_pupumimi);
@name(post_pupumimi);
[Postfix 100] ""++"" ""--"";
@node(pre_pupumimi);
@name(pre_pupumimi); 
[Prefix 100] ""++"" ""--"";
@node(factorial);
@name(factorial);
[Postfix 100] ""!"";
@node(dollar);
@name(dollar);
[Prefix 100] ""$"";
@node(entier);
@name(entier); 
[Operand] INT;
";
        var model = _processor.CompileModel(grammar);
        Check.That(model.IsOK).IsTrue();
        var x = _processor.Compile(grammar);
        Check.That(model.IsOK).IsTrue();
        var generatedParser = _processor.GenerateParser(grammar, "ns", "object");
        Check.That(generatedParser.IsOK).IsTrue();
        Check.That(generatedParser.Result).IsNotNull();
        Check.That(generatedParser.Result.Parser).Contains("object factorial(object value");
        Check.That(generatedParser.Result.Parser).Contains("object entier(Token<l> p0)");
        Check.That(generatedParser.Result.Parser).Contains("public object mimi(object left, Token<l> oper, object right)");
        var tree = _processor.GetSyntaxTree(grammar, "1+1-8");
        Check.That(tree.IsOK).IsTrue();
        Check.That(tree.Result).IsNotNull();
        var dump = tree.Result.Dump("", "    ");
        var lines = dump.GetLines();
        Check.That(lines).Contains("        + entier ");
        Check.That(lines).Contains("    + pupuce ");
        Check.That(lines).Contains("        + mimi ");
        var json = tree.Result.ToJson();
    
    }

    [Fact]
    public void TestIndentedWhile()
    {
        //
        // 1. compile model form meta.txt
        //
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/indentedWhile.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "IndentedWhileGrammar");
        Check.That(model).IsOkResult();
        Check.That(model.Value).IsNotNull();

        string program = @"
# factorial 
r:=1
i:=1
while i == 11 do 
    r := r * i
    print r
    print i
    i := i + 1
    
v1 := 48
v2 := 152
fstring := $""v1 :> {v1} < v2 :> {v2} < v3 :> {v1+v2} <  v4 :>{$""hello,"".$"" world""}< v5 :>{(? b -> $""true"" | $""false"")}< - end""
print fstring

return 100     
";
        var t = builder.Getz(grammar,program,"indentedWhileGrammar",new List<(string format, SyntaxTreeProcessor processor)>() {{("DOT",ParserBuilder.SyntaxTreeToDotGraph)}});
        Check.That(t).IsOkResult();
    }

    [Fact]
    public void TestProcessorCallback()
    {
        //
        // 1. compile model form meta.txt
        //
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/indentedWhile.txt");
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "IndentedWhileGrammar");
        Check.That(model).IsOkResult();
        Check.That(model.Value).IsNotNull();

        string program = @"
# factorial 
r:=1
i:=1
while i == 11 do 
    r := r * i
    print r
    print i
    i := i + 1
    
v1 := 48
v2 := 152
fstring := $""v1 :> {v1} < v2 :> {v2} < v3 :> {v1+v2} <  v4 :>{$""hello,"".$"" world""}< v5 :>{(? b -> $""true"" | $""false"")}< - end""
print fstring

return 100     
";
        
        List<string> messages = new List<string>();
        var logger = (string message) =>
        {
            messages.Add(message);
        };
        var r = _processor.GetJson(grammar, program, logger);
        Check.That(r.IsOK).IsTrue();
        Check.That(messages).CountIs(4);
    }


    [Fact]
    public void TestDecompileMeta()
    {
        var decompiler = new Decompiler();
        var decompiled = decompiler.Decompile(typeof(CLIToken), typeof(CLIParser));
        Check.That(decompiled).IsNotNull().And.IsNotEmpty();
        var rc = _processor.CompileModel(decompiled);
        Check.That(rc).IsOkCliResult();
        var selfCompiled = _processor.GetJson(decompiled, decompiled);
        Check.That(selfCompiled).IsOkCliResult();
    }

    [Fact]
    public void TestCustomId()
    {
        var builder = new ParserBuilder();
        var grammar = @"
genericLexer customLexer;
[CustomId] ID : ""_A-Za-z"" ""---_-_0-9A-Za-z."";
parser customParser;
-> root : ID+; 
";
        
        var model = builder.CompileModel(grammar, "CustomIdGrammar");
        Check.That(model).IsOkResult();
        Check.That(model.Value).IsNotNull();

        string program = @"
foo_bar.baz.qux-45.quux_66 hello _world666
";
        var t = builder.Getz(grammar,program,"CustomIdGrammar",new List<(string format, SyntaxTreeProcessor processor)>() {{("DOT",ParserBuilder.SyntaxTreeToDotGraph)}});
        Check.That(t).IsOkResult();

        var generated = _processor.GenerateParser(grammar,"ns","object");
        Check.That(generated).IsNotNull();
        Check.That(generated).IsOkCliResult();
        Check.That(generated.Result.Lexer).IsNotNull().And.IsNotEmpty();
        Check.That(generated.Result.Lexer).Contains("[CustomId(\"_A-Za-z\", \"---_-_0-9A-Za-z.\")]");

        var extracted = _processor.ExtractGrammar(generated.Result.Parser, generated.Result.Lexer);
        Check.That(extracted).IsOkCliResult();
        Check.That(extracted.Result).Contains("[CustomId] ID : \"_A-Za-z\" \"---_-_0-9A-Za-z.\";");
    }
}