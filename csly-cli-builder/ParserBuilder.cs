using System.Reflection;
using System.Reflection.Emit;
using csly.cli.model;
using csly.cli.model.parser;
using csly.cli.parser;
using sly.buildresult;
using LexerBuilder = clsy.cli.builder.lexer.LexerBuilder;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.generator.visitor.dotgraph;
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
    
    public Type BuildExtensionType { get; set; }

    public Type LexerPostProcessType { get; set; }
    public ParserBuilder()
    {
        
    }

    
    
    private (object parserBuildResult, Type parserType, Type lexerType) BuildParser(Model model)
    {
        
        
        // TODO
        var(enumType, assemblyBuilder, moduleBuilder) = LexerBuilder.BuildLexerEnum(model.LexerModel);
        EnumType = enumType;
        ObjectType = typeof(object);
        TokenType = BuilderHelper.BuildGenericType(typeof(Token<>),EnumType);
        TokenListType = BuilderHelper.BuildGenericType(typeof(List<>),TokenType);
        ObjectListType = BuilderHelper.BuildGenericType(typeof(List<>),ObjectType);
        GroupType = BuilderHelper.BuildGenericType(typeof(sly.parser.parser.Group<,>), EnumType, ObjectType);
        OptionGroupType = BuilderHelper.BuildGenericType(typeof(ValueOption<>), GroupType);
        OptionType = typeof(ValueOption<object>);
        GroupListType = BuilderHelper.BuildGenericType(typeof(List<>), GroupType);
        BuildExtensionType = typeof(BuildExtension<>).MakeGenericType(EnumType);
        LexerPostProcessType = typeof(LexerPostProcess<>).MakeGenericType(EnumType);

        TypeBuilder typeBuilder = moduleBuilder.DefineType(DynamicParserName, TypeAttributes.Public, typeof(object));

        foreach (var rule in model.ParserModel.Rules)
        {
            BuildVisitor(typeBuilder,rule);
        }

        var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
        var il = constructorBuilder.GetILGenerator();        
        il.Emit(OpCodes.Ret);
        
        
        
        Type compiledType = typeBuilder.CreateType();
        return (BuildIt(compiledType, model.ParserModel.Root), compiledType, EnumType);
    }

    private Model CompileModel(string filename)
    {
        ParserBuilder<CLIToken, ICLIModel> builder = new ParserBuilder<CLIToken, ICLIModel>();
        var instance = new CLIParser();
        //TestLexer();

        var buildParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        if (buildParser.IsOk)
        {
            var content = File.ReadAllText(filename);
            var result = buildParser.Result.ParseWithContext(content, new ParserContext());
            if (result.IsError)
            {
                result.Errors.ForEach(x => Console.WriteLine(x.ErrorMessage));
            }
            else {
                Model model = result.Result as Model;
                return model;
            }
        }
        else
        {
            buildParser.Errors.ForEach(x => Console.WriteLine(x.Message));
        }

        return null;
    }

    public DotGraph GetDot(string modelSourceFileName, string source)
    {
        var model = CompileModel(modelSourceFileName);
        var buildResult = BuildParser(model);

        var parserType = typeof(Parser<,>).MakeGenericType(buildResult.lexerType,typeof(object));
        var buildResultType = typeof(BuildResult<>).MakeGenericType(parserType);
            
        var resultProperty = buildResultType.GetProperty("Result");
        var parser = resultProperty.GetValue(buildResult.parserBuildResult, null);

        var parseMethod = parserType.GetMethod("Parse", new[] { typeof(string), typeof(string) });
        var result = parseMethod.Invoke(parser, new object[] { source, null });

        var ParseResultType = typeof(ParseResult<,>).MakeGenericType(buildResult.lexerType, typeof(object));
        var parseResultProp = ParseResultType.GetProperty("SyntaxTree");
        var syntaxTree = parseResultProp.GetValue(result);
            
        var graphvizType = typeof(GraphVizEBNFSyntaxTreeVisitor<>).MakeGenericType(buildResult.lexerType);
        var visitor = graphvizType.GetConstructor(new Type[] { }).Invoke(new object[]{});
            
            
        var visited = graphvizType
            .GetMethod("VisitTree",new Type[]{syntaxTree.GetType()})
            .Invoke(visitor, new object[]{syntaxTree});
            
        var graph = graphvizType
            .GetProperty("Graph")
            .GetValue(visitor);

        var dot = (graph as DotGraph);

        return dot;
    }
    
    private object BuildIt(Type parserType, string root)
    {
        var constructor = parserType.GetConstructor(Type.EmptyTypes);
        var instance = constructor.Invoke(new object?[]{});
        var builderType = typeof(ParserBuilder<,>);
        builderType = builderType.MakeGenericType(EnumType, ObjectType);
        var builderconstructor =
            builderType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
        var builder = builderconstructor.Invoke(new object?[] { });
        var buildMethod = builderType.GetMethod("BuildParser", new Type[] { ObjectType,typeof(ParserType),typeof(string),BuildExtensionType,LexerPostProcessType});
        
        // object parserInstance, ParserType parserType,
        // string rootRule, BuildExtension<IN> extensionBuilder = null, LexerPostProcess<IN> lexerPostProcess = null
        
        var x = buildMethod.Invoke(builder, new object?[] { instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, root,null,null }); // TODO
        return x;
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

        var parameters = rule.Clauses.Select(x => BuildTypeParameter(x)).ToArray();
        
        var methodBuilder = AddMethod(builder, rule.Key, parameters); 
       
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


    private Type BuildTypeParameter(IClause clause)
    {
        switch (clause)
        {
            case TerminalClause terminal:
            {
                return TokenType;
            }
            case NonTerminalClause:
            {
                return ObjectType;
            }
            case GroupClause:
            {
                return GroupType;
            }
            case OptionClause option:
            {
                switch (option.Clause) 
                {
                    case TerminalClause:
                    {
                        return TokenType;
                    }
                    case NonTerminalClause:
                    {
                        return OptionType;
                    }
                    case GroupClause:
                    {
                        return OptionGroupType;
                    }
                    default:
                    {
                        return OptionType;
                    }
                }
            }
            case ManyClause many:
            {
                switch (many.Clause)
                {
                    case TerminalClause:
                    {
                        return TokenListType;
                    }
                    case NonTerminalClause:
                    {
                        return ObjectListType;
                    }
                    case GroupClause:
                    {
                        return GroupListType;
                    }
                    default:
                    {
                        return ObjectListType;
                    }
                }
            }
            default:
            {
                return ObjectType;
            }
                
        }
    }
    
    
}
