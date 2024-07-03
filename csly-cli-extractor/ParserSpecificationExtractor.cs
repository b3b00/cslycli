using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace specificationExtractor;

public class ParserSpecificationExtractor
{

    private string Operation(string affix, string associativity, string precedence, string token)
    {
        if (affix == "PreFix")
        {
            if (token.StartsWith("'") && token.EndsWith("'"))
            {
                token = $@"""{token.Substring(1, token.Length - 1)}""";
            }
            return $"[Prefix {precedence}] {token};";
        }
        else if (affix == "PostFix")
        {
            return $"[Postfix {precedence}] {token};";
        }
        else if (affix == "InFix")
        {
            if (associativity == "Right")
            {
                return $"[Right {precedence}] {token};";
            }
            else if (associativity == "Left")
            {
                return $"[Left {precedence}] {token};";
            }
        }

        return "operation";
    }
    
    
    private string Rule(string type, string rootRule,string nodeName, params string[] args)
    {

        StringBuilder builder = new StringBuilder();
        if (!string.IsNullOrEmpty(nodeName))
        {
            builder.AppendLine($@"@node(""{nodeName}"")");
        }
        // Production
        if (type == "Production")
        {
            var rule = args[0];
            var split =  rule.Split(new[] { ':' });
            var nonTerminal = split[0].Trim();
            bool isRoot = nonTerminal == rootRule;
            builder.AppendLine($"{(isRoot? "-> ":"")}{args[0]};");
        }
        // Prefix
        if (type == "Prefix")
        {
            builder.AppendLine(Operation("PreFix", "", args[2], args[0]));
        }
        // Postfix
        if (type == "Postfix")
        {
            builder.AppendLine(Operation("PostFix", "", args[2], args[0]));
        }
        // Infix
        if (type == "Infix")
        {
            builder.AppendLine(Operation("InFix", args[1], args[2], args[0]));
        }
        // Operation
        if (type == "Operation")
        {
            string token = args[0];
            string precedence = args[3];
            string affix = args[1];
            string associativity = args[2];
            builder.AppendLine(Operation(affix, associativity, precedence, token));
        }
        return builder.ToString();
    }
    
    public string ExtractFromFile(string parserCsFileName)
    {
        
        var programText = File.ReadAllText(parserCsFileName);
        return ExtractFromSource(programText);
        
    }
    
    public string ExtractFromSource(string parserSource)
    {
        StringBuilder builder = new StringBuilder();
        
        var tree = CSharpSyntaxTree.ParseText(parserSource);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        if (root.DescendantNodes().FirstOrDefault(x => x.IsKind(SyntaxKind.ClassDeclaration)) is ClassDeclarationSyntax parserDecl)
        {
            string rootRule = null;
            var parserAttributes = parserDecl.AttributeLists.SelectMany(x => x.Attributes).ToList();
            var parserRootAttribute = parserAttributes.FirstOrDefault(x => x.Name.ToString() == "ParserRoot");
            if (parserRootAttribute != null)
            {
                rootRule = parserRootAttribute.ArgumentList.Arguments[0].ToString().Replace("\"","");
            }
            bool useMemo = parserAttributes.Exists(x => x.Name.ToString() == "UseMemoization");
            bool broadWindow = parserAttributes.Exists(x => x.Name.ToString() == "BroadenTokenWindow");
            bool autoCloseIndentation = parserAttributes.Exists(x => x.Name.ToString() == "AutoCloseIndentations");
            
            builder.AppendLine($"parser {parserDecl.Identifier.Text};").AppendLine();
            if (useMemo)
            {
                builder.AppendLine("[UseMemoization]");
            }

            if (broadWindow)
            {
                builder.AppendLine("[BroadenTokenWindow]");
            }

            if (autoCloseIndentation)
            {
                builder.AppendLine("[AutoCloseIndentations]");
            }
            var methods= parserDecl.Members.Where(x => x is MethodDeclarationSyntax).Cast<MethodDeclarationSyntax>().ToList();
            foreach (var method in methods)
            {
                if (method.AttributeLists.Any())
                {
                    var attributes = method.AttributeLists;
                    
                    // TODO : if attributes contains Production and Operand 
                    var allAttributes = attributes.SelectMany(x => x.Attributes).ToList();
                    var operand = allAttributes.FirstOrDefault(x => x.Name.ToString().Contains("Operand"));
                    
                    var nodeNameAttribute = allAttributes.FirstOrDefault(x => x.Name.ToString().Contains("NodeName"));
                    string nodeName = null;
                    if (nodeNameAttribute != null)
                    {
                        nodeName = nodeNameAttribute.ArgumentList.Arguments.First().Expression.ExprToString();
                    }
                    
                    foreach (var attr in attributes.SelectMany(x => x.Attributes))
                    {
                        if (attr.Name.ToString().Contains("Operand"))
                        {
                            continue;
                        }
                        if (attr.Name.ToString().Contains("NodeName"))
                        {
                            continue;
                        }
                        string[] pstrings = { };
                        if (attr?.ArgumentList?.Arguments != null && attr.ArgumentList.Arguments.Any())
                        {
                            pstrings = attr.ArgumentList.Arguments.Select(x => x.Expression.ExprToString()).ToArray();
                        }

                        if (operand != null)
                        {
                            builder.Append("[Operand] ");
                        }

                        builder.AppendLine(Rule(attr.Name.ToString(),rootRule, nodeName,pstrings));
                    }
                }
            }
        }


        return builder.ToString();
    }
}