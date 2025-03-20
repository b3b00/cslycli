using clsy.cli.builder.parser;
using csly.cli.model.parser;
using NFluent;
using Xunit;

namespace CliTests;

public class RepeatTests
{
    [Fact]
    public void TestCompile()
    {
        var grammar = @"
genericLexer MinimalLexer;
[AlphaId] ID ;
[Sugar] COLON : "":"" ;

parser RepeatParser;

-> root : ID{3} COLON ID{10-20}  ;
";
        var builder = new ParserBuilder();
        var model = builder.CompileModel(grammar, "RepeatParser");
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
}