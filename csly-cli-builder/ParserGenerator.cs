using System.Text;
using csly.cli.model;
using csly.cli.model.parser;

namespace clsy.cli.builder;

public class ParserGenerator
{
    public static string GenerateParser(Model model, string nameSpace, string output)
    {
        var head = GetHeader(model.ParserModel.Name, nameSpace);
        var body = GetBody(model.ParserModel, model.ParserModel.Name, model.LexerModel.Name, output);
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

    private static string GetBody(ParserModel model, string parser, string lexer, string output)
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
            else
            {
                builder.AppendLine(GetProduction(rule, parser));
                builder.AppendLine(GetVisitor(rule, parser, lexer, output));
                builder.AppendLine();
            }
        }
        return builder.ToString();
    }

 
    private static string GetProduction(Rule rule, string parser)
    {
        StringBuilder builder = new StringBuilder();
        if (rule.IsOperand)
        {
            builder.AppendLine("\t\t[Operand]");
        }
        builder.Append($"\t\t[Production(\"{GetRuleString(rule, parser)}\")]");
        return builder.ToString();
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

    private static string GetRuleString(Rule rule, string parser)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(rule.NonTerminalName).Append(" : ");
        builder.Append(string.Join(" ", rule.Clauses.Select(x => GetClause(x, parser))));
        return builder.ToString();
    }

    private static string GetClause(IClause clause, string parser)
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

    public static string GetVisitor(Rule rule, string parser, string lexer, string output)
    {
        return GetVisitorHeader(rule, parser, lexer, output) + GetVisitorBody(output);
    }
    
    public static string GetVisitorBody(string output)
    {
        return $@"
        {{
            return default({output});
        }}";
    }
    
    public static string GetVisitorHeader(Rule rule, string parser, string lexer, string output)
    {
        string name = rule.RuleString.Replace(" ","").Replace(":","_").Replace("*","");

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
    
    public static string GetClauseType(IClause clause, string parser, string lexer, string output)
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
                builder.Append(string.Join(" | ", choice.Choices.Select(x => GetClause(x, parser))));
                builder.Append(" ]");
                return builder.ToString();
            }
            case ManyClause many:
            {
                return $"List<{GetClauseType(many.Clause, parser, lexer,output)}>";
            }
            case OptionClause opt:
            {
                if (opt.Clause is NonTerminalClause)
                {
                    return $"ValueOption<{GetClauseType(clause,parser,lexer,output)}>";
                }
                else if (clause is TerminalClause)
                {
                    return GetClauseType(clause, parser, lexer,output);
                }
                else if (clause is GroupClause)
                {
                    return $"ValueOption<{GetClauseType(clause, parser, lexer,output)}>";
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