using System.Collections.Generic;

namespace csly.cli.model.tree
{
    public class ManySyntaxNode : SyntaxNode 
    {
        public ManySyntaxNode(string name) : base(name, new List<ISyntaxNode>())
        {
        }
        
        public ManySyntaxNode(string name, List<ISyntaxNode> children) : base(name, children)
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
