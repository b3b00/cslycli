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
        return new OptionSyntaxNode(option.Name, option.Children.Select(x => Untype(x)).ToList());
    }

    public SyntaxNode Untype(SyntaxNode<T> node)
    {
        var n = new SyntaxNode(node.Name, node.Children.Select(x => Untype(x)).ToList());
        n.Operation = Untype(node.Operation);
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