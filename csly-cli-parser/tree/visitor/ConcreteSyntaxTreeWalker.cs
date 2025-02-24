

using cli.sly.model.tree.visitor;
using csly.cli.model.tree;

namespace clsy.cli.model.tree.visitor
{
    public class ConcreteSyntaxTreeWalker<OUT>
    {
        
        public IConcreteSyntaxTreeVisitor<OUT> Visitor { get; set; }

        public ConcreteSyntaxTreeWalker(IConcreteSyntaxTreeVisitor<OUT> visitor)
        {
            Visitor = visitor;
        } 
        
        private OUT VisitLeaf(SyntaxLeaf leaf)
        {
            if (leaf.Token.IsIndent)
            {
                return Visitor.VisitLeaf(leaf.Token);
            }
            else if (leaf.Token.IsUnIndent)
            {
                return Visitor.VisitLeaf(leaf.Token);
            }
            else if (leaf.Token.IsExplicit)
            {
                return Visitor.VisitLeaf(leaf.Token);
            }
            return Visitor.VisitLeaf(leaf.Token);
        }
        
        public OUT Visit(ISyntaxNode n)
        {
            switch (n)
            {
                case SyntaxLeaf leaf:
                    return VisitLeaf(leaf);
                case GroupSyntaxNode node:
                    return Visit(node);
                case ManySyntaxNode node:
                    return Visit(node);
                case OptionSyntaxNode node:
                    return Visit(node);
                case SyntaxNode node:
                    return Visit(node);
                case SyntaxEpsilon epsilon:
                {
                    return Visitor.VisitEpsilon();
                }
                default:
                    return Visitor.VisitLeaf(new Token() {TokenID = "",SpanValue="NULL".ToCharArray()});
            }
        }

        private OUT Visit(GroupSyntaxNode node)
        {
            return Visit(node as SyntaxNode);
        }

        private OUT Visit(OptionSyntaxNode node)
        {
            var child = node.Children != null && node.Children.Any<ISyntaxNode>() ? node.Children[0] : null;
            OUT optionChild = default;
            if (child == null || node.IsEmpty)
            {
                optionChild = Visitor.VisitOptionNode(false, default(OUT));
            }
            else
            {
                optionChild = Visit(child);
            }

            return optionChild;
        }


        private OUT Visit(SyntaxNode node)
        {
            
            var children = new List<OUT>();

            foreach (var n in node.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }

           
            //return callback(node, children);
            return Visitor.VisitNode(node,children);
        }
        
        private OUT Visit(ManySyntaxNode manyNode)
        {
            
            var children = new List<OUT>();

            foreach (var n in manyNode.Children)
            {
                var v = Visit(n);

                children.Add(v);
            }

           
            //return callback(node, children);
            return Visitor.VisitManyNode(manyNode,children);
        }

        
    }
}