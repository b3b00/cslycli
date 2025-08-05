using sly.parser.syntax.tree;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace csly.cli.model.tree;




public class TreeUntyper<T,O> where T : struct, Enum
{
    public static ISyntaxNode? Untype(ISyntaxNode<T,O> node)
    {
        return node switch
        {
            GroupSyntaxNode<T,O> group => Untype(group),
            ManySyntaxNode<T,O> many => Untype(many),
            SyntaxLeaf<T,O> leaf => Untype(leaf),
            OptionSyntaxNode<T,O> option => Untype(option),
            SyntaxNode<T,O> n => Untype(n),
            _ => null
        };
    }



    private static SyntaxLeaf? Untype(SyntaxLeaf<T,O> leaf)
    {
        return new SyntaxLeaf(Untype(leaf.Token), leaf.Discarded);
    }

    private static OptionSyntaxNode Untype(OptionSyntaxNode<T,O> option)
    {
        var n = new OptionSyntaxNode(option.Name, option.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(option.Operation);
        n.IsEpsilon = option.IsEpsilon;
        n.IsByPassNode = option.IsByPassNode;
        return n;
    }

    private static SyntaxNode Untype(SyntaxNode<T,O> node)
    {
        var n = new SyntaxNode(node.Name, node.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(node.Operation);
        n.IsEpsilon = node.IsEpsilon;
        n.IsByPassNode = node.IsByPassNode;
        
        return n;
    }
    
    private static SyntaxNode Untype(ManySyntaxNode<T,O> node)
    {
        var n = new ManySyntaxNode(node.Name, node.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(node.Operation);
        n.IsEpsilon = node.IsEpsilon;
        n.IsByPassNode = node.IsByPassNode;
        return n;
    }
    
    private static SyntaxNode Untype(GroupSyntaxNode<T,O> group)
    {
        var n = new GroupSyntaxNode(group.Name, group.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(group.Operation);
        n.IsEpsilon = group.IsEpsilon;
        n.IsByPassNode = group.IsByPassNode;
        return n;
    }


    private static OperationMetaData Untype(OperationMetaData<T,O> operation)
    {
        if (operation != null)
        {
            return new OperationMetaData(operation.Precedence, operation.Associativity, operation.Affix,
                operation.OperatorToken.ToString());
        }

        return null;
    }
    
    private static Token Untype(Token<T> token)
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