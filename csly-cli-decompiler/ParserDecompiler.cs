using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using sly;
using sly.parser.generator;

namespace decompiler;

public static class MethodInfoExtensions
{
    public static List<T> GetAttributes<T>(this MethodInfo method)
    {
        var attributes = method.GetCustomAttributes().ToList<Attribute>();
        var attr = attributes.Where(x => x is T).Cast<T>();
        return attr.ToList() ;
    }

    public static string GetEnumValue(this Type enumType, int value)
    {
        var values = Enum.GetValues(enumType);
        foreach (Enum enumVal in values)
        {
            var val = Convert.ToInt32(enumVal);
            if (val == value)
            {
                return enumVal.ToString();
            }
        }

        return "";

    }
}


public class ParserDecompiler
{
    public ParserDecompiler()
    {
        
    }

    public string DecompileParser(Type lexerType, Type parserType)
    {
        var parser = GetParser(parserType,lexerType);
        return parser;
    }
  
    private string GetParser(Type type, Type lexerType)
    {
        string root = null;
        bool useMemo = false;
        bool broadWindow = false;
        bool autoCloseIndentations = false;
        
        var parserRootAttribute = type.GetCustomAttribute<ParserRootAttribute>();
        if (parserRootAttribute != null)
        {
            root = parserRootAttribute.RootRule;
        }

        useMemo = type.GetCustomAttribute<UseMemoizationAttribute>() != null;
        broadWindow = type.GetCustomAttribute<BroadenTokenWindowAttribute>() != null;
        autoCloseIndentations = type.GetCustomAttribute<AutoCloseIndentationsAttribute>() != null;
        
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"parser {type.Name};").AppendLine();

        if (useMemo)
        {
            builder.AppendLine("[UseMemoization]");
        }

        if (broadWindow)
        {
            builder.AppendLine("[BroadenTokenWindow]");
        }
        
        if (autoCloseIndentations)
        {
            builder.AppendLine("[AutoCloseIndentations]");
        }
        
        var methods = type.GetMethods().ToList();
        foreach (var method in methods)
        {
            var productions = method.GetAttributes<ProductionAttribute>();
            var operands = method.GetAttributes<OperandAttribute>();
            var operations = method.GetAttributes<OperationAttribute>();

            foreach (var operation in operations)
            {
                builder.AppendLine(GetOperation(operation,lexerType));
            }

            bool isOperand = operands.Any();
            foreach (var production in productions)
            {
                builder.AppendLine(GetProduction(production, isOperand, root));
            }
        }

        return builder.ToString();
    }

    private string GetProduction(ProductionAttribute production, bool isOperand, string? rootRule)
    {
        var split =  production.RuleString.Split(new[] { ':' });
        var nonTerminal = split[0].Trim();
        bool isRoot = nonTerminal == rootRule;   
        return $"{(isRoot ? "-> ":"")}{(isOperand ? "[Operand] ":"")}{production.RuleString};";
    }

    private string GetOperation(OperationAttribute operation, Type lexerType)
    {
        string token = "";
        
        if (operation.IsStringToken)
        {
            token = operation.StringToken;
        }
        else if (operation.IsIntToken)
        {
            token = lexerType.GetEnumValue(operation.IntToken);
        }
        

        if (operation.Affix == Affix.InFix)
        {
            var tag = operation.Assoc == Associativity.Left ? "Left" : "Right";
            return $"[{tag} {operation.Precedence}] {token};";
        }

        if (operation.Affix == Affix.PreFix)
        {
            return $"[Prefix {operation.Precedence}] {token};";
        }
        if (operation.Affix == Affix.PostFix)
        {
            if (token.StartsWith("'") && token.EndsWith("'"))
            {
                token = token.Substring(1, token.Length - 2);
            }
            return $@"[Postfix {operation.Precedence}] ""{token}"";";
        }
        return "";
    }
}