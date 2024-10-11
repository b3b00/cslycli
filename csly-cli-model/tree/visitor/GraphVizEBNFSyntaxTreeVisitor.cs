using System.Diagnostics.CodeAnalysis;
using cli.sly.model.tree.visitor;
using cli.sly.model.tree.visitor.dotgraph;
using clsy.cli.model.tree.visitor.dotgraph;
using csly.cli.model.tree;


namespace clsy.cli.model.tree.visitor
{
    [ExcludeFromCodeCoverage]
    public class GraphVizEBNFSyntaxTreeVisitor  : IConcreteSyntaxTreeVisitor<DotNode>
    {
        public DotGraph Graph { get; private set; }

        public GraphVizEBNFSyntaxTreeVisitor()
        {
            Graph = new DotGraph("syntaxtree", true);
        }

        private int NodeCounter = 0;

        public DotNode VisitTree(ISyntaxNode root)
        {
            Graph = new DotGraph("syntaxtree", true);
            
            ConcreteSyntaxTreeWalker<DotNode> walker = new ConcreteSyntaxTreeWalker<DotNode>(this);
            var dot =  walker.Visit(root);
            return dot;
        }

        private DotNode Node(string label, bool nodeIsByPassNode)
        {
            var shape = nodeIsByPassNode ? "ellipse" : "mrecord";
            var style = nodeIsByPassNode ? "dotted" : "solid";
            var node = new DotNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = shape,
                Style = style,
                Label = label,
                FontColor = "black",
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }

        public DotNode VisitOptionNode(bool exists, DotNode child)
        {
            if (!exists)
            {
                return VisitLeaf(new Token() { TokenID = "ε" });
            }
            return child;
        }

        public DotNode VisitNode(SyntaxNode node, IList<DotNode> children)
        {
            DotNode result = null;

            result = Node(GetNodeLabel(node),node.IsByPassNode);
            //children.ForEach(c =>
            foreach (var child in children)
            {
                if (child != null) // Prevent arrows with null destinations
                {
                    var edge = new DotArrow(result, child)
                    {
                        // Set all available properties
                        ArrowHeadShape = "none"
                    };
                    Graph.Add(edge);
                    //Graph.Add(child);
                }
            }
            return result;
        }

        public DotNode VisitManyNode(ManySyntaxNode node, IList<DotNode> children)
        {
            DotNode result = null;

            result = Node(GetNodeLabel(node),node.IsByPassNode);
            Graph.Add(result);
            //children.ForEach(c =>
            foreach (var child in children)
            {
                if (child != null) // Prevent arrows with null destinations
                {
                    var edge = new DotArrow(result, child)
                    {
                        // Set all available properties
                        ArrowHeadShape = "none"
                    };
                    Graph.Add(edge);
                }
            }
            return result;
        }

        public DotNode VisitEpsilon()
        {
            return VisitLeaf(new Token() { TokenID = "", SpanValue = "epsilon".ToCharArray() });
        }

       

        public DotNode VisitLeaf(Token token)
        {
            if (token.IsIndent)
            {
                return Leaf(token.TokenID, "INDENT>>");
            }
            else if (token.IsUnIndent)
            {
                return Leaf(token.TokenID, "<<UNINDENT");
            }
            else if (token.IsExplicit)
            {
                return Leaf(token.Value);
            }
            return Leaf(token.TokenID, token.Value);
        }

        private DotNode Leaf(string type, string value)
        {
            string label = type.ToString();
            if (label == "0")
            {
                label = "";
            }
            else
            {
                label += "\n";
            }
            var esc = value.Replace("\"", "\\\"");
            if (!string.IsNullOrEmpty(esc))
            {
                label += "\\\"" + esc + "\\\"";
            }
            var node = new DotNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = "doublecircle",
                Label = label,
                FontColor = "",
                Style = "",
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }
        
        private DotNode Leaf(string value)
        {
            string label = "";
            var esc = value.Replace("\"", "\\\"");
            label += "\\\"" + esc + "\\\"";
            var node = new DotNode(NodeCounter.ToString())
            {
                // Set all available properties
                Shape = "doublecircle",
                Label = label,
                FontColor = "",
                Style = "",
                Height = 0.5f
            };
            NodeCounter++;
            Graph.Add(node);
            return node;
        }
        
        
        private string GetNodeLabel(SyntaxNode node)
        {
            string label = node.Name;
            return label;
        }


    }
}