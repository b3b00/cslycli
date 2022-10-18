using System.Text;
using csly.cli.model.parser;

namespace clsy.cli.builder;

public class ParserGenerator
{
    public static string GenerateParser(ParserModel model, string name, string nameSpace, string lexer, string output)
    {
        var head = GetHeader(name, nameSpace);
        var body = GetBody(model, lexer, output);
        var foot = getFooter();
        return head+"\n"+body+"\n"+foot;
    }

    
    
    private static string GetHeader(string name, string nameSpace)
    {
        return $@"
using sly.lexer;
using sly.parser.generator;
using System.Collections.Generic;

namespace {nameSpace} {{

    public class {name} {{
";
    }

    private static string getFooter()
    {
        return @"
    }
}";
    }

    private static string GetBody(ParserModel model, string lexer, string output)
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
            else if (rule is OperandRule operand)
            {
                builder.AppendLine(GetProduction(operand, lexer, output));
                builder.AppendLine(GetVisitor(operand, lexer, output));
                builder.AppendLine();
            }
            else
            {
                builder.AppendLine(GetProduction(rule));
                builder.AppendLine(GetVisitor(rule, lexer, output));
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }


    private static string GetProduction(OperandRule operand, string lexer, string output)
    {
        return $@"
        [Operand]
        [Production(""operand_{operand.Name} : {operand.Name}"")]";
    }
    
    private static string GetProduction(Rule rule)
    {
        return $"\t\t[Production(\"{GetRuleString(rule)}\")]";
    }
    
    private static string GetProduction(InfixRule rule)
    {
        return $"\t\t[Infix(\"{rule.Name}\", Associativity.{rule.Associativity}, {rule.Precedence})]";
    }
    
    private static string GetProduction(PrefixRule prefix)
    {
        return $"\t\t[Prefix(\"{prefix.Name}\", Associativity.Left, {prefix.Precedence})]";
    }
    
    
    #region rule string

    private static string GetRuleString(Rule rule)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(rule.NonTerminalName).Append(" : ");
        builder.Append(string.Join(" ", rule.Clauses.Select(x => GetClause(x))));
        return builder.ToString();
    }

    private static string GetClause(IClause clause)
    {
        switch (clause)
        {
            case NonTerminalClause nt:
            {
                return nt.NonTerminalName;
            }
            case TerminalClause t:
            {
                return t.Name;
            }
            case GroupClause grp:
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("(");
                builder.Append(string.Join(" ", grp.Clauses.Select(x => GetClause(x))));
                builder.Append(")");
                return builder.ToString();
            }
            case ChoiceClause choice:
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("[ ");
                builder.Append(string.Join(" | ", choice.Choices.Select(x => GetClause(x))));
                builder.Append(" ]");
                return builder.ToString();
            }
            case OneOrMoreClause oneOr:
            {
                return GetClause(oneOr.Clause) + "+";
            }
            case ZeroOrMoreClause zeroOr:
            {
                return GetClause(zeroOr.Clause) + "+";
            }
            case OptionClause opt:
            {
                return GetClause(opt.Clause) + "?";
            }
            default:
            {
                return "";
            }
        }
        
        
    }

    #endregion
    
    #region visitors

    
    public static string GetVisitor(OperandRule operand, string lexer, string output)
    {
        // TODO : check if either token or value
        var paramType = operand.IsToken ? $"Token<{lexer}>": output;
        return $@"
        public {output} operand_{operand.Name}({paramType} value) {{
            return value;
        }}";
    }

    public static string GetVisitor(PrefixRule prefix, string lexer, string output)
    {
        return $@"
        public {output} {prefix.Name}(Token<{lexer}> oper, {output} value) {{
            return value;
        }}";
    }
    
    public static string GetVisitor(InfixRule infix, string lexer, string output)
    {
        return $@"
        public {output} {infix.Name}({output} left, Token<{lexer}> oper, {output} right) {{
            return left;
        }}";
    }

    public static string GetVisitor(Rule rule, string lexer, string output)
    {
        return GetVisitorHeader(rule, lexer, output) + "\n" + GetVisitorBody(output);
    }
    
    public static string GetVisitorBody(string output)
    {
        return $@"
        {{
            return default({output});
        }}";
    }
    
    public static string GetVisitorHeader(Rule rule, string lexer, string output)
    {
        string name = rule.RuleString.Replace(" ","").Replace(":","_").Replace("*","");

        string parameters = "";
        for (int i = 0; i < rule.Clauses.Count; i++)
        {
            var clause = rule.Clauses[i];
            var type = GetClauseType(clause, lexer, output);
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
    
    public static string GetClauseType(IClause clause, string lexer, string output)
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
                StringBuilder builder = new StringBuilder();
                builder.Append("[ ");
                builder.Append(string.Join(" | ", choice.Choices.Select(x => GetClause(x))));
                builder.Append(" ]");
                return builder.ToString();
            }
            case ManyClause many:
            {
                return $"List<{GetClauseType(many.Clause,lexer,output)}>";
            }
            case OptionClause opt:
            {
                if (opt.Clause is NonTerminalClause)
                {
                    return $"ValueOption<{GetClauseType(clause,lexer,output)}>";
                }
                else if (clause is TerminalClause)
                {
                    return GetClauseType(clause,lexer,output);
                }
                else if (clause is GroupClause)
                {
                    return $"ValueOption<{GetClauseType(clause,lexer,output)}>";
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