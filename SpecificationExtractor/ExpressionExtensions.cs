using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpecificationExtractor;

public static class ExpressionExtensions
{
    public static string ExprToString(this ExpressionSyntax expr)
    {
        switch (expr)
        {
            case LiteralExpressionSyntax lit:
            {
                if (lit.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    return lit.Token.ValueText;
                }

                if (lit.IsKind(SyntaxKind.NumericLiteralExpression))
                {
                    return lit.Token.Value.ToString();
                }

                return "";

            }
            case MemberAccessExpressionSyntax memberAccess:
            {
                string name = memberAccess.Name.Identifier.Text;
                return name;
            }
            case CastExpressionSyntax cast:
            {
                var typeSyntax = cast.Type;
                if (typeSyntax is PredefinedTypeSyntax predef && predef.Keyword != null) 
                {
                    return cast.Expression.ExprToString();    
                }

                return "";
            }
            default:
            {
                return "don't know !";
            }
        }

    }
}