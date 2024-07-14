using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using sly.parser.generator;


namespace csly.cli.model.tree
{
    
    public class OperationMetaData
    {
        
        public OperationMetaData(int precedence, sly.parser.generator.Associativity assoc,sly.parser.generator.Affix affix, string oper)
        {
            Precedence = precedence;
            Associativity = assoc;
            OperatorToken = oper;
            Affix = affix;
        }

        public int Precedence { get; set; }

        public sly.parser.generator.Associativity Associativity { get; set; }

        public sly.parser.generator.Affix Affix { get; set; }

        public bool IsBinary => Affix == Affix.InFix;

        public bool IsUnary => Affix != Affix.InFix;

        public bool IsExplicitOperatorToken => !string.IsNullOrEmpty(OperatorToken);

        public string OperatorToken { get; set; }

        public override string ToString()
        {
            return $"{OperatorToken} / {Affix} : {Precedence} / {Associativity}";
        }
    }
    
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
                    if (IsBinaryOperationNode && IsBinaryOperationNode)
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

        public string Dump(string tab)
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

            builder.AppendLine($"{tab}+ {Name} {(IsByPassNode ? "===":"")}");

            foreach (var child in Children)
            {
                builder.AppendLine($"{child.Dump(tab + "\t")}");
            }

            return builder.ToString();
        }

        public string ToJson(int index = 0)
        {
            StringBuilder builder = new StringBuilder();


            builder.Append($@"""{index}.{Name}");
            if (IsByPassNode)
            {
                builder.Append("--");
            }

            builder.AppendLine(@""" : {");

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                builder.Append(child.ToJson(i));
                if (i < Children.Count - 1)
                {
                    builder.Append(",");
                }

                builder.AppendLine();
            }

            builder.Append("}");

            return builder.ToString();
        }


        #endregion
    }
}
