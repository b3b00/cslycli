using System.Diagnostics.CodeAnalysis;
using sly.lexer;

namespace csly.cli.model.parser
{
    public class LeadingToken
    {
        public string ExplicitToken { get; set; }
        
        public bool IsExplicitToken { get; set; }

        public LeadingToken()
        {
            TokenId = tokenId;
            IsExplicitToken = false;
        }
        
        public LeadingToken(IN tokenId, string explicitToken)
        {
            TokenId = tokenId;
            ExplicitToken = explicitToken;
            IsExplicitToken = true;
        }

        public bool Match(Token<IN> token)
        {
            if (IsExplicitToken)
            {
                return ExplicitToken == token.Value;
            }
            else
            {
                return TokenId.Equals(token.TokenID);
            }
        }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (IsExplicitToken)
            {
                return $"'{ExplicitToken}'";
            }
            else
            {
                return TokenId.ToString();
            }
        }
    }
}