using System.Collections.Generic;

namespace sly.parser.syntax.tree
{
    public class ManySyntaxNode : SyntaxNode 
    {
        public ManySyntaxNode(string name) : base(name, new List<ISyntaxNode>())
        {
        }

        public bool IsManyTokens { get; set; }

        public bool IsManyValues { get; set; }

        public bool IsManyGroups { get; set; }


        public void Add(ISyntaxNode child)
        {
            Children.Add(child);
        }
    }
}
