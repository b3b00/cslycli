using System.Collections.Generic;
using csly.cli.model.tree;
using sly.lexer;
using sly.parser.syntax.tree;

namespace cli.sly.model.tree.visitor
{
    public interface IConcreteSyntaxTreeVisitor<OUT> 
    {
        OUT VisitOptionNode(bool exists, OUT child);
        OUT VisitNode(SyntaxNode node, IList<OUT> children);
        OUT VisitManyNode(ManySyntaxNode node, IList<OUT> children);

        OUT VisitEpsilon();
        OUT VisitLeaf(Token token);
    }
}