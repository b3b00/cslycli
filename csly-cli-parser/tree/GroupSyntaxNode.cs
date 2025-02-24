using System.Collections.Generic;

namespace csly.cli.model.tree
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
