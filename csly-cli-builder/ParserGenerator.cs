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
                builder.AppendLine(GetVisitor(infix, lexer, output));
                builder.AppendLine();
            } 
            else if (rule is PrefixRule prefix)
            {
                builder.AppendLine(GetProduction(prefix));
                builder.AppendLine(GetVisitor(prefix, lexer, output));
                builder.AppendLine();
                builder.AppendLine();
            }
            else if (rule is PostfixRule postfix)
            {
                builder.AppendLine(GetProduction(postfix));
                builder.AppendLine(GetVisitor(postfix, lexer, output));
                builder.AppendLine();
                builder.AppendLine();
            }
            else
            {
                builder.AppendLine(GetProduction(rule, parser));
                builder.AppendLine(GetVisitor(rule, parser, lexer, output));
                builder.AppendLine();
            }
        }
        return builder.ToString();
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
        return $"\t\t[Infix(\"{rule.Name}\", Associativity.{rule.Associativity}, {rule.Precedence})]";
    }
    
    private  string GetProduction(PrefixRule prefix)
    {
        return $"\t\t[Prefix(\"{prefix.Name}\", Associativity.Left, {prefix.Precedence})]";
    }
    
    private  string GetProduction(PostfixRule postfix)
    {
        return $"\t\t[Postfix(\"{postfix.Name}\", Associativity.Left, {postfix.Precedence})]";
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
        string name = prefix.Name;
        if (prefix.IsExplicit)
        {
            name = $"prefix_{explicitPrefixCounter}";
            explicitPrefixCounter++;
        }
        
        return $@"
        public {output} {name}(Token<{lexer}> oper, {output} value) {{
            return value;
        }}";
    }
    
    
    private  int explicitPostfixCounter = 0;
    public  string GetVisitor(PostfixRule postfix, string lexer, string output)
    {
        string name = postfix.Name;
        if (postfix.IsExplicit)
        {
            name = $"postfix_{explicitPostfixCounter}";
            explicitPostfixCounter++;
        }
        
        return $@"
        public {output} {name}({output} value, Token<{lexer}> oper) {{
            return value;
        }}";
    }
    
    public  string GetVisitor(InfixRule infix, string lexer, string output)
    {
        return $@"
        public {output} {infix.Name}({output} left, Token<{lexer}> oper, {output} right) {{
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
                else
                {
                    name.Append('_');
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
                        return output;
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