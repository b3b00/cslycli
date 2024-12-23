using sly.parser.syntax.tree;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace csly.cli.model.tree;




public class TreeUntyper<IN, OUT> where IN :struct
{
    public static ISyntaxNode? Untype(ISyntaxNode<IN, OUT> node)
    {
        return node switch
        {
            GroupSyntaxNode<IN, OUT> group => Untype(group),
            ManySyntaxNode<IN, OUT> many => Untype(many),
            SyntaxLeaf<IN, OUT> leaf => Untype(leaf),
            OptionSyntaxNode<IN, OUT> option => Untype(option),
            SyntaxNode<IN, OUT> n => Untype(n),
            _ => null
        };
    }



    private static SyntaxLeaf? Untype(SyntaxLeaf<IN, OUT> leaf)
    {
        return new SyntaxLeaf(Untype(leaf.Token), leaf.Discarded);
    }

    private static OptionSyntaxNode Untype(OptionSyntaxNode<IN, OUT> option)
    {
        var n = new OptionSyntaxNode(option.Name, option.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(option.Operation);
        n.IsEpsilon = option.IsEpsilon;
        n.IsByPassNode = option.IsByPassNode;
        return n;
    }

    private static SyntaxNode Untype(SyntaxNode<IN, OUT> node)
    {
        var n = new SyntaxNode(node.Name, node.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(node.Operation);
        n.IsEpsilon = node.IsEpsilon;
        n.IsByPassNode = node.IsByPassNode;
        
        return n;
    }
    
    private static SyntaxNode Untype(ManySyntaxNode<IN, OUT> node)
    {
        var n = new ManySyntaxNode(node.Name, node.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(node.Operation);
        n.IsEpsilon = node.IsEpsilon;
        n.IsByPassNode = node.IsByPassNode;
        return n;
    }
    
    private static SyntaxNode Untype(GroupSyntaxNode<IN, OUT> group)
    {
        var n = new GroupSyntaxNode(group.Name, group.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(group.Operation);
        n.IsEpsilon = group.IsEpsilon;
        n.IsByPassNode = group.IsByPassNode;
        return n;
    }


    private static OperationMetaData Untype(OperationMetaData<IN, OUT> operation)
    {
        if (operation != null)
        {
            return new OperationMetaData(operation.Precedence, operation.Associativity, operation.Affix,
                operation.OperatorToken.ToString());
        }

        return null;
    }
    
    private static Token Untype(Token<IN> token)
    {
        return new Token(token.TokenID.ToString(), token.SpanValue, token.Position, token.CommentType, token.Channel,
            token.IsWhiteSpace, token.DecimalSeparator)
        {
            IsExplicit = token.IsExplicit,
            End = token.End,
            IsWhiteSpace = token.IsWhiteSpace,
            IsEOL = token.IsEOL,
            IsEOS = token.IsEOS,
            Label = token.Label,
            PositionInTokenVisibleFlow = token.PositionInTokenVisibleFlow,
            PositionInTokenFlow = token.PositionInTokenFlow
        };
    }
    
    
}