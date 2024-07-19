using sly.parser.generator;

namespace csly.cli.model.tree;

public class OperationMetaData
{
        
    public OperationMetaData(int precedence, sly.parser.generator.Associativity assoc,sly.parser.generator.Affix affix, string oper)
    {
        Precedence = precedence;
        Associativity = assoc;
        OperatorToken = oper;
        Affix = affix;
    }

    public int Precedence { get; set; }

    public sly.parser.generator.Associativity Associativity { get; set; }

    public sly.parser.generator.Affix Affix { get; set; }

    public bool IsBinary => Affix == Affix.InFix;

    public bool IsUnary => Affix != Affix.InFix;

    public bool IsExplicitOperatorToken => !string.IsNullOrEmpty(OperatorToken);

    public string OperatorToken { get; set; }

    public override string ToString()
    {
        return $"{OperatorToken} / {Affix} : {Precedence} / {Associativity}";
    }
}