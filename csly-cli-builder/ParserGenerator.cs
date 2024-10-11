using System.Text;
using csly.cli.model;
using csly.cli.model.parser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using sly.parser.syntax.grammar;

namespace clsy.cli.builder;

public class ParserGenerator
{

    public ParserGenerator()
    {
        
    }
    
    public  string GenerateParser(Model model, string nameSpace, string output)
    {
        var head = GetHeader(model.ParserModel, nameSpace);
        var body = GetBody(model.ParserModel, model.ParserModel.Name, model.LexerModel.Name, output);
        var foot = getFooter();
        
        var source = head+"\n"+body+"\n"+foot;
        
        var tree = CSharpSyntaxTree.ParseText(source);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
        var prettyPrintedSource = root.NormalizeWhitespace()
            .SyntaxTree
            .GetText(CancellationToken.None)
            .ToString();

        return prettyPrintedSource;
        
        
    }

    
    
    private  string GetHeader(ParserModel model, string nameSpace)
    {
        string optimizations = "";
        if (model.UseMemoization)
        {
            optimizations += "    [UseMemoization]\n";
        }

        if (model.BroadenTokenWindow)
        {
            optimizations += "    [BroadenTokenWindow]";
        }

        if (model.AutoCloseIndentations)
        {
            optimizations += "    [AutoCloseIndentations]";
        }
        
        return $@"
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;
using sly.parser.parser;

namespace {nameSpace} {{

    [ParserRoot(""{model.Root}"")]
{optimizations}
    public class {model.Name} {{
";
    }

    private  string getFooter()
    {
        return @"
    }
}";
    }

    private  string GetBody(ParserModel model, string parser, string lexer, string output)
    {
        StringBuilder builder = new StringBuilder();
        foreach (var rule in model.Rules)
        {
            if (rule is InfixRule infix)
            {
                builder.AppendLine(GetProduction(infix));
                SetAttributes(infix, builder);
                builder.AppendLine(GetVisitor(infix, lexer, output));
                builder.AppendLine();
            }
            else if (rule is ManyInfixRule infixes)
            {
                builder.AppendLine(GetProduction(infixes));
                SetAttributes(infixes, builder);
                builder.AppendLine(GetVisitor(infixes, lexer, output));
                builder.AppendLine();
            }
            else if (rule is ManyPrefixRule prefixes)
            {
                builder.AppendLine(GetProduction(prefixes));
                SetAttributes(prefixes, builder);
                builder.AppendLine(GetVisitor(prefixes, lexer, output));
                builder.AppendLine();
            } 
            else if (rule is ManyPostfixRule postfixes)
            {
                builder.AppendLine(GetProduction(postfixes));
                SetAttributes(postfixes, builder);
                builder.AppendLine(GetVisitor(postfixes, lexer, output));
                builder.AppendLine();
            } 
            else if (rule is PrefixRule prefix)
            {
                builder.AppendLine(GetProduction(prefix));
                SetAttributes(prefix,builder);
                builder.AppendLine(GetVisitor(prefix, lexer, output));
                builder.AppendLine();
                builder.AppendLine();
            }
            else if (rule is PostfixRule postfix)
            {
                builder.AppendLine(GetProduction(postfix));
                SetAttributes(postfix,builder);
                builder.AppendLine(GetVisitor(postfix, lexer, output));
                builder.AppendLine();
                builder.AppendLine();
            }
            else
            {
                builder.AppendLine(GetProduction(rule, parser));
                SetAttributes(rule, builder);
                builder.AppendLine(GetVisitor(rule, parser, lexer, output));
                builder.AppendLine();
            }

            
        }
        return builder.ToString();
    }

    private void SetAttributes(Rule rule, StringBuilder builder)
    {
        if (rule.TryGetNodeName(out var nodeName))
        {
            builder.AppendLine($@"[NodeName(""{nodeName}"")]");
        }
    }


    private  string GetProduction(Rule rule, string parser)
    {
        StringBuilder builder = new StringBuilder();
        if (rule.IsOperand)
        {
            builder.AppendLine("\t\t[Operand]");
        }
        builder.Append($"\t\t[Production(\"{GetRuleString(rule, parser)}\")]");
        return builder.ToString();
    }
    
    private  string GetProduction(InfixRule rule)
    {
        string name = rule.IsExplicit ? $"'{rule.Name}'" : rule.Name;
        return $"\t\t[Infix(\"{name}\", Associativity.{rule.Associativity}, {rule.Precedence})]";
    }
    private  string GetProduction(ManyInfixRule rule)
    {
        StringBuilder builder = new StringBuilder();
        foreach (var infix in rule.Infixes)
        {
            string name = infix.IsExplicit ? $"'{infix.Name}'" : infix.Name;
            builder.AppendLine($"\t\t[Infix(\"{name}\", Associativity.{infix.Associativity}, {infix.Precedence})]");    
        }

        return builder.ToString();
    }
    
    private  string GetProduction(ManyPrefixRule rule)
    {
        StringBuilder builder = new StringBuilder();
        foreach (var prefix in rule.Prefixes)
        {
            string name = prefix.IsExplicit ? $"'{prefix.Name}'" : prefix.Name;
            builder.AppendLine($"\t\t[Prefix(\"{name}\", Associativity.Left, {prefix.Precedence})]");    
        }

        return builder.ToString();
    }
    
    private  string GetProduction(ManyPostfixRule rule)
    {
        StringBuilder builder = new StringBuilder();
        foreach (var postfix in rule.Postfixes)
        {
            string name = postfix.IsExplicit ? $"'{postfix.Name}'" : postfix.Name;
            builder.AppendLine($"\t\t[Postfix(\"{name}\", Associativity.Left, {postfix.Precedence})]");    
        }

        return builder.ToString();
    }
    
    private  string GetProduction(PrefixRule prefix)
    {
        string name = prefix.IsExplicit ? $"'{prefix.Name}'" : prefix.Name;
        return $"\t\t[Prefix(\"{name}\", Associativity.Left, {prefix.Precedence})]";
    }
    
    
    
    private  string GetProduction(PostfixRule postfix)
    {
        string name = postfix.IsExplicit ? $"'{postfix.Name}'" : postfix.Name;
        return $"\t\t[Postfix(\"{name}\", Associativity.Left, {postfix.Precedence})]";
    }
    
    
    #region rule string

    private  string GetRuleString(Rule rule, string parser)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(rule.NonTerminalName).Append(" : ");
        builder.Append(string.Join(" ", rule.Clauses.Select(x => GetClause(x, parser))));
        return builder.ToString();
    }

    private  string GetClause(IClause clause, string parser)
    {
        switch (clause)
        {
            case NonTerminalClause nt:
            {
                return nt.NonTerminalName;
            }
            case TerminalClause t:
            {
                return t.ToString();
            }
            case GroupClause grp:
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("(");
                builder.Append(string.Join(" ", grp.Clauses.Select(x => GetClause(x, parser))));
                builder.Append(")");
                return builder.ToString();
            }
            case ChoiceClause choice:
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("[ ");
                builder.Append(string.Join(" | ", choice.Choices.Select(x => GetClause(x, parser))));
                builder.Append(" ]");
                return builder.ToString();
            }
            case OneOrMoreClause oneOr:
            {
                return GetClause(oneOr.Clause, parser) + " +";
            }
            case ZeroOrMoreClause zeroOr:
            {
                return GetClause(zeroOr.Clause, parser) + " *";
            }
            case OptionClause opt:
            {
                return GetClause(opt.Clause, parser) + "?";
            }
            default:
            {
                return "";
            }
        }
        
        
    }

    #endregion
    
    #region visitors

    private  int explicitPrefixCounter = 0;
    
    public  string GetVisitor(PrefixRule prefix, string lexer, string output)
    {
        string name = "";
        if (!prefix.TryGetMethodName(out name)) {
            name = prefix.Name;
            if (prefix.IsExplicit)
            {
                name = $"prefix_{explicitPrefixCounter}";
                explicitPrefixCounter++;
            }
        }

        return $@"
        public {output} {name}(Token<{lexer}> oper, {output} value) {{
            return value;
        }}";
    }
    
    public  string GetVisitor(ManyPrefixRule prefixes, string lexer, string output)
    {
        string name = prefixes.GetName(ref explicitPrefixCounter); 

        return $@"
        public {output} {name}(Token<{lexer}> oper, {output} value) {{
            return value;
        }}";
    }
    
    
    private  int explicitPostfixCounter = 0;
    public  string GetVisitor(ManyPostfixRule postfixes, string lexer, string output)
    {
        string name = postfixes.GetName(ref explicitPostfixCounter); 
        
        return $@"
        public {output} {name}({output} value, Token<{lexer}> oper) {{
            return value;
        }}";
    }
    
    public  string GetVisitor(PostfixRule postfix, string lexer, string output)
    {
        string name = ""; 
        if (!postfix.TryGetMethodName(out name))
        {
            name = postfix.Name;
            if (postfix.IsExplicit)
            {
                name = $"postfix_{explicitPostfixCounter}";
                explicitPostfixCounter++;
            }
        }
        
        return $@"
        public {output} {name}({output} value, Token<{lexer}> oper) {{
            return value;
        }}";
    }
    
    public  string GetVisitor(ManyInfixRule infixes, string lexer, string output)
    {
        string name = infixes.GetName(ref explicitInfixCounter); 

        return $@"
        public {output} {name}({output} left, Token<{lexer}> oper, {output} right) {{
            return left;
        }}";
    }
    
    private  int explicitInfixCounter = 0;
    public  string GetVisitor(InfixRule infix, string lexer, string output)
    {
        string name = ""; 
        if (!infix.TryGetMethodName(out name)) {
            name = infix.Name;
            if (infix.IsExplicit)
            {
                name = $"infix_{explicitInfixCounter}";
                explicitInfixCounter++;
            }
        }

        return $@"
        public {output} {name}({output} left, Token<{lexer}> oper, {output} right) {{
            return left;
        }}";
    }

    public  string GetVisitor(Rule rule, string parser, string lexer, string output)
    {
        return GetVisitorHeader(rule, parser, lexer, output) + GetVisitorBody(output);
    }
    
    public  string GetVisitorBody(string output)
    {
        return $@"
        {{
            return default({output});
        }}";
    }


    private  Dictionary<string, int> visitorNames = new Dictionary<string, int>();
    public  string GetVisitorHeader(Rule rule, string parser, string lexer, string output)
    {
        StringBuilder name = new StringBuilder();
        string methodName = "";
        if (rule.TryGetMethodName(out methodName))
        {
            name.Append(methodName);
        }
        else
        {
            foreach (var c in rule.RuleString)
            {
                if (Char.IsDigit(c) || char.IsLetter(c))
                {
                    name.Append(c);
                }
                else if (c != '_')
                {
                    if (name[name.Length - 1] != '_')
                    {
                        name.Append('_');
                    }
                }
            }
        }

        int count = 0;
        if (visitorNames.TryGetValue(name.ToString(), out count))
        {
            count++;
            visitorNames[name.ToString()] = count;
            name.Append($"_{count}");
        }
        else
        {
            visitorNames[name.ToString()] = count;    
        }

        string parameters = "";
        for (int i = 0; i < rule.Clauses.Count; i++)
        {
            var clause = rule.Clauses[i];
            var type = GetClauseType(clause, parser, lexer, output);
            var p = $"p{i}";
            parameters += $"{type} {p}";
            if (i < rule.Clauses.Count - 1)
            {
                parameters += ", ";
            }
        }
        
        
        return
            $"\t\tpublic {output} {name}({parameters})";
    }
    
    public  string GetClauseType(IClause clause, string parser, string lexer, string output)
    {
        switch (clause)
        {
            case NonTerminalClause:
            {
                return output;
            }
            case TerminalClause:
            {
                return $"Token<{lexer}>";
            }
            case GroupClause:
            {
                return $"Group<{lexer},{output}>";
            }
            case ChoiceClause choice:
            {
                var f = choice.Choices.First();
                var isTerminal = f is TerminalClause;
                if (isTerminal)
                {
                    return $"Token<{lexer}>";
                }
                else
                {
                    return output;
                }
                
            }
            case ManyClause many:
            {
                return $"List<{GetClauseType(many.Clause, parser, lexer,output)}>";
            }
            case OptionClause opt:
            {
                if (opt.Clause is NonTerminalClause nt)
                {
                    return $"ValueOption<{GetClauseType(nt,parser,lexer,output)}>";
                }
                else if (opt.Clause is TerminalClause t)
                {
                    return $"Token<{lexer}>";
                }
                else if (opt.Clause is GroupClause grp)
                {
                    return $"ValueOption<{GetClauseType(grp, parser, lexer,output)}>";
                }
                else if (opt.Clause is ChoiceClause choice)
                {
                    if (choice.IsTerminalChoice)
                    {
                        return $"Token<{lexer}>";
                    }
                    else
                    {
                        return $"ValueOption<{output}>";
                    }
                }

                return output;
            }
            default:
            {
                return output;
            }
        }
    } 
    
    
    #endregion
    
}