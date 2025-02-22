using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace specificationExtractor;

public static class ExpressionExtensions
{
    public static string ExprToString(this ExpressionSyntax expr)
    {
        switch (expr)
        {
            case IdentifierNameSyntax idName:
            {
                return idName.Identifier.Text;
            }
            case LiteralExpressionSyntax lit:
            {
                if (lit.IsKind(SyntaxKind.CharacterLiteralExpression))
                {
                    return lit.Token.Value.ToString();
                }
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
                if (typeSyntax is PredefinedTypeSyntax predef && predef.Keyword.ToString() == "int") 
                {
                    return cast.Expression.ExprToString();    
                }

                return "";
            }
            case InterpolatedStringExpressionSyntax interpol:
            {
                return interpol.Contents.ToString();
            }
            default:
            {
                return "don't know !";
            }
        }

    }
}