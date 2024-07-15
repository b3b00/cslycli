using System.Collections.Generic;
using System.Reflection;

namespace csly.cli.model.tree
{
    public class OptionSyntaxNode : SyntaxNode 
    {
        public bool IsGroupOption { get; set; } = false;

        public OptionSyntaxNode(string name, List<ISyntaxNode> children = null) : base(
            name, children)
        { }
    }
}
