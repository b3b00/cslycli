using System.Reflection;
using System.Reflection.Emit;
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
    
    public Type ObjectType { get; set; }
    public ParserBuilder()
    {
        
    }
    
    public (object parserBuildResult, Type parserType) BuildParser(Model model)
    {
        // TODO
        EnumType = LexerBuilder.BuildLexerEnum(model.LexerModel);
        ObjectType = typeof(object);
        TokenType = BuilderHelper.BuildGenericType(typeof(Token<>),EnumType);
        TokenListType = BuilderHelper.BuildGenericType(typeof(List<>),TokenType);
        ObjectListType = BuilderHelper.BuildGenericType(typeof(List<>),ObjectType);
        GroupType = BuilderHelper.BuildGenericType(typeof(sly.parser.parser.Group<,>), EnumType, ObjectType);
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
        if (rule.IsRule)
        {
            AddProduction(builder,rule);
        }

        if (rule.IsPrefix && rule is PrefixRule prefix)
        {
            AddPrefix(builder, prefix);
        }
        if (rule.IsInfix && rule is InfixRule infix)
        {
            AddInfix(builder, infix);
        }
        if (rule.IsOperand && rule is OperandRule operand)
        {
            AddOperand(builder, operand);
        }
    }
    
    
    private  void AddProduction(TypeBuilder builder, Rule rule)
    {

        var methodBuilder = AddMethod(builder, rule.Key, Type.EmptyTypes); 
            
        
        // ConstructorInfo constructorInfo = attributeType.GetConstructor(
        //     new Type[1] { typeof(string) });
        //     
        // CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
        //     constructorInfo, new object[] { rule.RuleString });
        //
        // builder.SetCustomAttribute(customAttributeBuilder);
        _AddProductionAttribute(methodBuilder, rule.RuleString);
    }
    
    private  void _AddProductionAttribute(MethodBuilder builder, string rule)
    {
        Type attributeType = typeof(ProductionAttribute);
        
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[1] { typeof(string) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { rule });

        builder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private  void AddPrefix(TypeBuilder builder, PrefixRule prefix)
    {

        var methodBuilder = AddMethod(builder, $"prefix_{prefix.Name}", TokenType, ObjectType);
        
        Type attributeType = typeof(PrefixAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[3] { typeof(string),typeof(Associativity),typeof(int) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { prefix.Name,Associativity.Left,prefix.Precedence });

        methodBuilder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private void AddInfix(TypeBuilder builder, InfixRule infix)
    {
        //var methodBuilder =  builder.DefineMethod($"infix_{infix.Name}_{infix.Associativity}_{infix.Precedence}", default);

        var methodBuilder = AddMethod(builder, $"infix_{infix.Name}_{infix.Associativity}_{infix.Precedence}",
            ObjectType, TokenType, ObjectType); 
        
        Type attributeType = typeof(InfixAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[3] { typeof(string),typeof(Associativity),typeof(int) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { infix.Name,infix.Associativity,infix.Precedence });

        methodBuilder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private void AddOperand(TypeBuilder builder, OperandRule operand)
    {

         var paramtype = operand.IsToken ? TokenType : ObjectType;
        // var methodBuilder = builder.DefineMethod($"operand_{operand.Name}",
        //     MethodAttributes.Public,
        //     CallingConventions.Standard,
        //     ObjectType, new[] { paramtype });
        // 
        //     
        // methodBuilder.GetILGenerator().Emit(OpCodes.Ldnull);
        // methodBuilder.GetILGenerator().Emit(OpCodes.Ret);

        var methodBuilder = AddMethod(builder, $"operand_{operand.Name}", paramtype);
        Type attributeType = typeof(OperandAttribute);
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[] {  });
        
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] {  });
        
        methodBuilder.SetCustomAttribute(customAttributeBuilder);
        
        _AddProductionAttribute(methodBuilder,$"operand_{operand.Name} : {operand.Name}");

        
    }

    public MethodBuilder AddMethod(TypeBuilder builder, string name, params Type[] parameterTypes)
    {
        
        var methodBuilder = builder.DefineMethod(name,
            MethodAttributes.Public,
            CallingConventions.Standard,
            ObjectType, parameterTypes);
            
        methodBuilder.GetILGenerator().Emit(OpCodes.Ldnull);
        methodBuilder.GetILGenerator().Emit(OpCodes.Ret);

        return methodBuilder;
    }
    
    
}