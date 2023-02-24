using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using sly.lexer;
using sly.parser.generator.visitor;
using sly.parser.syntax.tree;
using  Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Label = Microsoft.Msagl.Drawing.Label;
using Node = Microsoft.Msagl.Drawing.Node;
using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;

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
        var svgRootNode = walker.Visit(tree);
        
        _graph.CreateGeometryGraph();
        
        // Now the drawing graph elements point to the corresponding geometry elements, 
        // however the node boundary curves are not set.
        // Setting the node boundaries
        foreach (var n in _graph.Nodes) {
            // Ideally we should look at the drawing node attributes, and figure out, the required node size
            // I am not sure how to find out the size of a string rendered in SVG. Here, we just blindly assign to each node a rectangle with width 60 and height 40, and round its corners.
            n.GeometryNode.BoundaryCurve = CurveFactory.CreateRectangleWithRoundedCorners(60, 40, 3, 2, new Point(0, 0));
        }
           
        AssignLabelsDimensions(_graph);

        LayoutHelpers.CalculateLayout(_graph.GeometryGraph, new SugiyamaLayoutSettings(), null);
        var svg = PrintSvgAsString(_graph);
        
        return svg;
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
            var edge =_graph.AddEdge(node.Id, child.Id).Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            //edge.GeometryEdge = e;
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
        node.LabelText = label;
        return node;
    }
    
    private void AssignLabelsDimensions(Graph graph) {
        

        foreach (var node in graph.Nodes)       
        {
            if (node.Label != null)
            {
                node.Label.Width = node.Width * 0.6;
                node.Label.Height = 40;
            }
        }
    }
    
    static string PrintSvgAsString(Graph drawingGraph) {
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        var svgWriter = new SvgGraphWriter(writer.BaseStream, drawingGraph);
        svgWriter.Write();
        ms.Position = 0;
        var sr = new StreamReader(ms);
        var svgString = sr.ReadToEnd();
        return svgString;
    }
}