using clsy.cli.builder.parser;
using csly_cli_api;
using csly.cli.model.parser;
using NFluent;
using Xunit;

namespace CliTests;

public class RepeatTests
{
    private string _grammar = @"
genericLexer MinimalLexer;
[AlphaId] ID ;
[Sugar] COLON : "":"" ;

parser RepeatParser;

-> root : ID{3} COLON ID{10-20}  ;
";
    
    private CslyProcessor _processor;

    public RepeatTests()
    {
        _processor = new CslyProcessor();
    }


    [Fact]
    public void TestCompile()
    {
        var builder = new ParserBuilder();
        var model = builder.CompileModel(_grammar, "RepeatParser");
        Check.That(model).IsOkModel();
        Check.That(model.Value.ParserModel.Rules).CountIs(1);
        var rule = model.Value.ParserModel.Rules[0];
        Check.That(rule.Clauses).CountIs(3);
        var clause1 = rule.Clauses[0];
        Check.That(clause1).IsInstanceOf<RepeatClause>();
        var repeat1 = clause1 as RepeatClause;
        Check.That(repeat1.Min).IsEqualTo(3);
        Check.That(repeat1.Max).IsEqualTo(3);
        var clause2 = rule.Clauses[2];
        Check.That(clause2).IsInstanceOf<RepeatClause>();
        var repeat2 = clause2 as RepeatClause;
        Check.That(repeat2.Min).IsEqualTo(10);
        Check.That(repeat2.Max).IsEqualTo(20);
        
        
    }
    
    [Fact]
    public void TestRun()
    {
        var result = _processor.Parse(_grammar,"a b c : d e f g h i j k l m n o p");
        Check.That(result.IsOK).IsTrue();
        // TODO : check the tree ?
    }
    
    [Fact]
    public void TestRunFail()
    {
        var result = _processor.Parse(_grammar,"a b c : d e f g h i j k l m n o p q r s t u v w x y z ");
        Check.That(result.IsOK).IsFalse();
        Check.That(result.Errors).CountIs(1);
        Check.That(result.Errors[0]).Contains("parse error : Erreur de syntaxe : 'x (line 0, column 48)' ID inattendu.");
    }
}