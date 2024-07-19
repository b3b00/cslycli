using sly.lexer;

namespace csly.cli.model.tree
{
    public class SyntaxLeaf : ISyntaxNode 
    {
        public SyntaxLeaf(Token token, bool discarded)
        {
            Token = token;
            Discarded = discarded;
        }

        public bool IsEpsilon => false;

        public Token Token { get;  }
        public bool Discarded { get; }
        public string Name => Token.TokenID.ToString();

        public bool HasByPassNodes { get; set; } = false;

        public string Dump(string currentTab, string singleTab)
        {
            if (Token.IsExplicit)
            {
                return $"{currentTab}+ '{Token.Value}' @{Token.PositionInTokenFlow}";    
            }
            
            return $"{currentTab}+ {Token.TokenID.ToString()} : '{Token.Value}' @{Token.PositionInTokenFlow}";
        }

        public string ToJson(int index = 0)
        {
            return $@"""{index}.{(Token.IsExplicit ? "explicit" :Token.TokenID.ToString())}"" : ""{Token.Value}""";
        }


    }
}
