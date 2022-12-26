using System.Reflection;
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

        var lexbuild = LexerBuilder.BuildLexer<CLIToken>(CLITokenExtensions.AddExtension);
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
        EmbeddedResourceFileSystem fs = new EmbeddedResourceFileSystem(Assembly.GetAssembly(typeof(Tests)));
        var grammar = fs.ReadAllText("/data/ext.txt");

        var builder = new ParserBuilder<CLIToken, ICLIModel>();
        var pb = builder.BuildParser(new CLIParser(), ParserType.EBNF_LL_RECURSIVE_DESCENT, "token",CLITokenExtensions.AddExtension);
        Check.That(pb.IsError).IsFalse();
        Check.That(pb.Result).IsNotNull();
        var r = pb.Result.Parse(grammar);
        Check.That(r).IsOkParsing();
        Check.That(r.IsError).IsFalse();

        var lexbuild = LexerBuilder.BuildLexer<CLIToken>(CLITokenExtensions.AddExtension);
        Check.That(lexbuild);
        Check.That(lexbuild.IsError).IsFalse();
        Check.That(lexbuild.Result).IsNotNull();
        var lexer = lexbuild.Result;
        var graph = (lexer as GenericLexer<CLIToken>).ToGraphViz();
        var lexed = lexer.Tokenize(grammar);
        Check.That(lexed).IsOkLexing();
        Check.That(lexed.IsError).IsFalse();
        var tokens = lexed.Tokens.Tokens;
        Check.That(tokens).Not.IsNullOrEmpty();
        Check.That(tokens).CountIs(20);

    }
    
    
      
}