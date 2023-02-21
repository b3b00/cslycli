using Microsoft.Msagl.Core.Layout;
using sly.lexer;
using sly.parser.generator.visitor;
using sly.parser.syntax.tree;
using  Microsoft.Msagl.Drawing;
using Label = Microsoft.Msagl.Drawing.Label;
using Node = Microsoft.Msagl.Drawing.Node;

namespace clsy.cli.builder;

public class SvgTreeBuilder<IN> : IConcreteSyntaxTreeVisitor<IN,Node> where IN : struct
{

    private Graph _graph;

    private int counter = 0;

    public SvgTreeBuilder()
    {
        _graph = new Graph("this is a graph");
        _graph.GeometryGraph = new GeometryGraph();
    }

    public string VisitTree(ISyntaxNode<IN> tree)
    {
        var walker = new ConcreteSyntaxTreeWalker<IN, Node>(this);
        var svgGraph = walker.Visit(tree);
        Stream stream = null;
        using (stream = new MemoryStream()) {
            var writer = new SvgGraphWriter(stream, _graph) {
                Precision = 4
            };
            writer.Write();
        }

        using (var reader = new StreamReader(stream))
        {
            var svg = reader.ReadToEnd();
            return svg;
        }
        
        return "nothing";
    }
    
    public Node VisitOptionNode(bool exists, Node child)
    {
        return child;
    }

    public Node VisitNode(SyntaxNode<IN> syntaxNode, IList<Node> children)
    {
        var node = BuildNode(syntaxNode.Name, false);
        foreach (var child in children)
        {
            var edge =_graph.AddEdge(node.Id, child.Id);
        }

        return node;
    }

    public Node VisitManyNode(ManySyntaxNode<IN> syntaxNode, IList<Node> children)
    {
        var node = BuildNode(syntaxNode.Name, false);
        foreach (var child in children)
        {
            _graph.AddEdge(node.Id, child.Id);
        }

        return node;
    }

    public Node VisitEpsilon()
    {
        return BuildNode("ε",true);
    }


    private string LeafLabel(IN type, string value)
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
        label += "\\\"" + esc + "\\\"";
        return label;
    }
    public Node VisitLeaf(Token<IN> token)
    {
        if (token.IsIndent)
        {
            return BuildNode(LeafLabel(token.TokenID, "INDENT>>"),true);
        }
        else if (token.IsUnIndent)
        {
            return BuildNode(LeafLabel(token.TokenID, "<<UNINDENT"),true);
        }
        else if (token.IsExplicit)
        {
            return BuildNode(token.Value,true);
        }
        return BuildNode(LeafLabel(token.TokenID, token.Value),true);
    }

    private Node BuildNode(string label, bool isLeaf=false)
    {
        var node = _graph.AddNode($"{(isLeaf ? "l_" : "n_")}{counter}");
        counter++;
        node.Label = new Label(label);
        return node;
    }
}