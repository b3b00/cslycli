using sly.parser.syntax.tree;
using sly.lexer;
using sly.parser.generator;
using sly.parser.syntax.grammar;

namespace csly.cli.model.tree;


public class TreeUntyper<T> where T :struct
{
    public ISyntaxNode? Untype(ISyntaxNode<T> node)
    {
        return node switch
        {
            GroupSyntaxNode<T> group => Untype(group),
            ManySyntaxNode<T> many => Untype(many),
            EmptyNode<T> empty => Untype(empty),
            SyntaxEpsilon<T> epsilon => Untype(epsilon),
            SyntaxLeaf<T> leaf => Untype(leaf),
            OptionSyntaxNode<T> option => Untype(option),
            SyntaxNode<T> n => Untype(n),
            _ => null
        };
    }

    public EmptyNode? Untype(EmptyNode<T> emptyNode)
    {
        return new EmptyNode();
    }

    public SyntaxLeaf? Untype(SyntaxLeaf<T> leaf)
    {
        return new SyntaxLeaf(Untype(leaf.Token), leaf.Discarded);
    }

    public SyntaxEpsilon? Untype(SyntaxEpsilon<T> epsilon)
    {
        return new SyntaxEpsilon();
    }

    public OptionSyntaxNode Untype(OptionSyntaxNode<T> option)
    {
        var n = new OptionSyntaxNode(option.Name, option.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(option.Operation);
        n.IsEpsilon = option.IsEpsilon;
        n.IsByPassNode = option.IsByPassNode;
        return n;
    }

    public SyntaxNode Untype(SyntaxNode<T> node)
    {
        var n = new SyntaxNode(node.Name, node.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(node.Operation);
        n.IsEpsilon = node.IsEpsilon;
        n.IsByPassNode = node.IsByPassNode;
        return n;
    }
    
    public SyntaxNode Untype(ManySyntaxNode<T> node)
    {
        var n = new ManySyntaxNode(node.Name, node.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(node.Operation);
        n.IsEpsilon = node.IsEpsilon;
        n.IsByPassNode = node.IsByPassNode;
        return n;
    }
    
    public SyntaxNode Untype(GroupSyntaxNode<T> group)
    {
        var n = new GroupSyntaxNode(group.Name, group.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(group.Operation);
        n.IsEpsilon = group.IsEpsilon;
        n.IsByPassNode = group.IsByPassNode;
        return n;
    }


    public OperationMetaData Untype(OperationMetaData<T> operation)
    {
        return new OperationMetaData(operation.Precedence, operation.Associativity, operation.Affix,
            operation.OperatorToken.ToString());
    }
    
    public Token Untype(Token<T> token)
    {
        return new Token(token.TokenID.ToString(), token.SpanValue, token.Position, token.CommentType, token.Channel, token.IsWhiteSpace, token.DecimalSeparator);
    }
    
    
}