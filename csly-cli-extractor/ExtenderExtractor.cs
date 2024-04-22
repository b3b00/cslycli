using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace specificationExtractor;

public class ExtenderExtractor
{

    private bool IsExtenderMethod(MethodDeclarationSyntax method, string[] types)
    {
        var parameters = method.ParameterList.ChildNodes().Cast<ParameterSyntax>().ToList();
        if (parameters.Count == types.Length)
        {
            for (int i = 0; i < types.Length; i++)
            {
                var expected = types[i];
                var p = parameters[i];
                var pType = p.Type;
                if (pType != null)
                {
                    if (pType is IdentifierNameSyntax id)
                    {
                        var typeName = id.Identifier.Text;
                        Console.WriteLine("checking {p0}");
                        if (typeName != expected)
                        {
                            return false;
                        }
                    }

                    if (pType is GenericNameSyntax genid)
                    {
                        var t = genid.Identifier.Text;
                        string args = string.Join(",", genid.TypeArgumentList.Arguments.Select(x => x.GetText()));
                        var fullName = $"{t}<{args}>";
                        if (fullName != expected)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    public string ExtractFromFile(string fileName, string lexerName)
    {
        string source = File.ReadAllText(fileName);
        return ExtractFromSource(source, lexerName);
    }
    public string ExtractFromSource(string source, string lexerName)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
        var extClass =
            root.DescendantNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.ClassDeclaration)) as ClassDeclarationSyntax;
        var methods = root.DescendantNodes()
            .Where(x => x.IsKind(SyntaxKind.MethodDeclaration))
            .Cast<MethodDeclarationSyntax>();
        if (methods != null && methods.Any())
        {
            methods = methods.Where(x => IsExtenderMethod(x,new[]{lexerName, "LexemeAttribute",$"GenericLexer<{lexerName}>"})).ToList();
            // TODO : here we have extension methods (note we can have many ! ) => should be specified by user ?
            Console.Write("looking");
            foreach (var method in methods)
            {
                ExtractMethod(method, lexerName);
            }
        }
        return "";
    }

    public void ExtractMethod(MethodDeclarationSyntax method, string lexerName)
    {
        var tokenName = (method.ParameterList.ChildNodes().First() as ParameterSyntax).Identifier.Text;
        
        var body = method.Body;
        var ifs = body.DescendantNodes().Where(x => x.IsKind(SyntaxKind.IfStatement));
        foreach (var ifStatement in ifs)
        {
            var condition = ifStatement.ChildNodes().First() as BinaryExpressionSyntax;
            var ifBlock = ifStatement.ChildNodes().Last() as BlockSyntax;
            if ((condition.Left as IdentifierNameSyntax).Identifier.Text == tokenName)
            {
                if (condition.Right is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Expression is IdentifierNameSyntax id && id.Identifier.Text == lexerName)
                    {
                        string token = memberAccess.Name.Identifier.Text;
                        ;
                        var statements = ifBlock.ChildNodes().Where(x => x.IsKind(SyntaxKind.ExpressionStatement)).ToList();
                        foreach (var statement in statements)
                        {
                            if (statement is ExpressionStatementSyntax access)
                            {
                                ;
                                // if (access.Name.Identifier.Text == "Goto")
                                // {
                                //     ;
                                // }
                            }    
                        }

                    }
                } 
                ;
            }
            var block = ifStatement.ChildNodes().Last() as BlockSyntax;
            Console.WriteLine("testing");
        }
        
    }
}