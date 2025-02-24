using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using sly.parser.generator;


namespace csly.cli.model.tree
{
    public class SyntaxNode : ISyntaxNode 
    {

        public SyntaxNode(string name, List<ISyntaxNode> children = null)
        {
            _isEpsilon = children == null || !children.Any() || (children.Count == 1 && children[0].IsEpsilon);
            Name = name;
            Children = children ?? new List<ISyntaxNode>();
        }

        private bool _isEpsilon = false;
        public bool IsEpsilon
        {
            get => _isEpsilon;
            set => _isEpsilon = value;
        }
        public List<ISyntaxNode> Children { get; }


        public bool IsByPassNode { get; set; } = false;

        public bool IsEmpty
        {
            get => Children == null || !Children.Any();
            set => throw new NotImplementedException();
        }

        public sly.parser.generator.Affix ExpressionAffix { get; set; }


        public bool Discarded => false;
        public string Name { get; set; }

        public bool HasByPassNodes { get; set; } = false;

        #region expression syntax nodes

        [JsonIgnore]
        public OperationMetaData Operation { get; set; } = null;

        public bool IsExpressionNode => Operation != null;

        public bool IsBinaryOperationNode => IsExpressionNode && Operation.Affix == sly.parser.generator.Affix.InFix;
        public bool IsUnaryOperationNode => IsExpressionNode && Operation.Affix != sly.parser.generator.Affix.InFix;
        public int Precedence => IsExpressionNode ? Operation.Precedence : -1;

        public sly.parser.generator.Associativity Associativity =>
            IsExpressionNode && IsBinaryOperationNode ? Operation.Associativity : Associativity.None;

        public bool IsLeftAssociative => Associativity == Associativity.Left;

        public ISyntaxNode Left
        {
            get
            {
                ISyntaxNode l = null;
                if (IsExpressionNode)
                {
                    var leftindex = -1;
                    if (IsBinaryOperationNode) leftindex = 0;
                    if (leftindex >= 0) l = Children[leftindex];
                }

                return l;
            }
            set
            {
                if (IsExpressionNode)
                {
                    if (IsBinaryOperationNode)
                    {
                        Children[0] = value;
                    }

                }
            }
        }

        public ISyntaxNode Right
        {
            get
            {
                ISyntaxNode r = null;
                if (IsExpressionNode)
                {
                    var rightIndex = -1;
                    if (IsBinaryOperationNode)
                        rightIndex = 2;
                    else if (IsUnaryOperationNode) rightIndex = 1;
                    if (rightIndex > 0) r = Children[rightIndex];
                }

                return r;
            }
            set
            {
                if (IsExpressionNode)
                {
                        var rightIndex = -1;
                        if (IsBinaryOperationNode)
                            rightIndex = 2;
                        else if (IsUnaryOperationNode) rightIndex = 1;
                        if (rightIndex > 0) Children[rightIndex] = value;
                }
            }
        }

        public string Dump(string initialTab, string singleTab)
        {
            StringBuilder builder = new StringBuilder();
            string expressionSuffix = "";
            if (Operation != null && Operation.IsBinary)
            {
                if (Operation.IsExplicitOperatorToken)
                {
                    expressionSuffix = Operation.OperatorToken;
                }
                else
                {
                    expressionSuffix = Operation.OperatorToken.ToString();
                }

                expressionSuffix = $">{expressionSuffix}<";
            }

            builder.AppendLine($"{initialTab}+ {Name} {(IsByPassNode ? "===" : "")}");

            var childTab = initialTab + (IsByPassNode ? "": singleTab);
            foreach (var child in Children)
            {
                builder.AppendLine($"{child.Dump(childTab,singleTab)}");
            }

            return builder.ToString();
        }

        public string ToJson(int index = 0)
        {
            StringBuilder builder = new StringBuilder();


            if (!IsByPassNode)
            {
                builder.Append($@"""{index}.{Name}");
                builder.AppendLine(@""" : {");
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                builder.Append(child.ToJson(index+i));
                if (i < Children.Count - 1)
                {
                    builder.Append(",");
                }

                builder.AppendLine();
            }

            if (!IsByPassNode)
            {
                builder.Append("}");
            }

            return builder.ToString();
        }


        #endregion
    }
}
