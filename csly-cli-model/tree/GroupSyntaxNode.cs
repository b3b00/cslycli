using System.Collections.Generic;

namespace sly.parser.syntax.tree
{
    public class GroupSyntaxNode : ManySyntaxNode
    {
        public GroupSyntaxNode(string name) : base(name)
        {
        }

        public GroupSyntaxNode(string name,  List<ISyntaxNode> children) : this(name)
        {
            Children.AddRange(children);
        }

    }
}
