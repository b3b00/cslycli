using System.Collections.Generic;
using System.Reflection;

namespace sly.parser.syntax.tree
{
    public class OptionSyntaxNode : SyntaxNode 
    {
        public bool IsGroupOption { get; set; } = false;

        public OptionSyntaxNode(string name, List<ISyntaxNode> children = null, MethodInfo visitor = null) : base(
            name, children, visitor)
        { }
    }
}
