using System.Reflection;
using System.Reflection.Emit;
using clsy.cli.builder.parser.cli.model;
using csly.cli.model;
using csly.cli.model.parser;
using LexerBuilder = clsy.cli.builder.lexer.LexerBuilder;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace clsy.cli.builder.parser;

public class ParserBuilder
{
    
    public static string DynamicParserName = "dynamicParser";

    public Type EnumType { get; set; }
    public Type TokenType { get; set; }
    
    public Type TokenListType { get; set; }
    
    public Type ObjectListType { get; set; }
    
    public Type OptionType { get; set; }
    
    public Type GroupType { get; set; }
    
    public Type OptionGroupType { get; set; }
    
    public Type GroupListType { get; set; }
    public ParserBuilder()
    {
        
    }
    
    public (object parserBuildResult, Type parserType) BuildParser(Model model)
    {
        // TODO
        EnumType = LexerBuilder.BuildLexerEnum(model.LexerModel);
        TokenType = BuilderHelper.BuildGenericType(typeof(Token<>),EnumType);
        TokenListType = BuilderHelper.BuildGenericType(typeof(List<>),TokenType);
        ObjectListType = BuilderHelper.BuildGenericType(typeof(List<>),typeof(object));
        GroupType = BuilderHelper.BuildGenericType(typeof(sly.parser.parser.Group<,>), EnumType, typeof(object));
        OptionGroupType = BuilderHelper.BuildGenericType(typeof(ValueOption<>), GroupType);
        OptionType = typeof(ValueOption<object>);
        GroupListType = BuilderHelper.BuildGenericType(typeof(List<>), GroupType);
        
        AppDomain currentDomain = AppDomain.CurrentDomain;

// Create a dynamic assembly in the current application domain,
// and allow it to be executed and saved to disk.

        AssemblyName aName = new AssemblyName(LexerBuilder.DynamicAssemblyName);

        var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(aName,
            AssemblyBuilderAccess.Run);


// Define a dynamic module in "TempAssembly" assembly. For a single-
// module assembly, the module has the same name as the assembly.
        ModuleBuilder moduleBuilder = dynamicAssembly.DefineDynamicModule(aName.Name);

// Define a public enumeration with the name "Elevation" and an 
// underlying type of Integer.
        TypeBuilder typeBuilder = moduleBuilder.DefineType(DynamicParserName, TypeAttributes.Public, typeof(int));

        foreach (var rule in model.ParserModel.Rules)
        {
            BuildVisitor(typeBuilder,rule);
        }
        
        
        Type finished = typeBuilder.CreateType();
        return (null, null);
    }

    private void BuildVisitor(TypeBuilder builder, Rule rule)
    {
        var methodBuilder =  builder.DefineMethod(rule.Key, default);

        if (rule.IsRule)
        {
            AddProduction(methodBuilder,rule.RuleString);
        }

        if (rule.IsPrefix && rule is PrefixRule prefix)
        {
            AddPrefix(methodBuilder, prefix.Name, prefix.Precedence);
        }
        if (rule.IsInfix && rule is InfixRule infix)
        {
            AddInfix(methodBuilder, infix.Name, infix.Associativity, infix.Precedence);
        }
        if (rule.IsOperand && rule is OperandRule operand)
        {
            AddOperand(methodBuilder, operand.Name);
        }
    }
    
    
    private static void AddProduction(MethodBuilder builder, string rule)
    {
        Type attributeType = typeof(ProductionAttribute);
            
        builder.GetILGenerator().Emit(OpCodes.Ldnull);
        builder.GetILGenerator().Emit(OpCodes.Ret);
        
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[1] { typeof(string) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { rule });

        builder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private static void AddPrefix(MethodBuilder builder, string token, int precedence)
    {
        Type attributeType = typeof(PrefixAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[3] { typeof(string),typeof(Associativity),typeof(int) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { token,Associativity.Left,precedence });

        builder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private static void AddInfix(MethodBuilder builder, string token, Associativity assoc, int precedence)
    {
        Type attributeType = typeof(InfixAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[3] { typeof(string),typeof(Associativity),typeof(int) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { token,assoc,precedence });

        builder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private static void AddOperand(MethodBuilder builder, string token)
    {
        Type attributeType = typeof(OperandAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[] {  });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] {  });
        
        AddProduction(builder,$"operand_{token} : {token}");

        builder.SetCustomAttribute(customAttributeBuilder);
    }
}