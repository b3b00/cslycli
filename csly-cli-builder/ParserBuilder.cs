using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using clsy.cli.builder.checker;
using csly.cli.model;
using csly.cli.model.parser;
using csly.cli.parser;
using Newtonsoft.Json;
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

public delegate string SyntaxTreeProcessor(Type parserType, Type lexerType, object tree);

public class ParserBuilder
{
    
    public string DynamicParserName { get; set; } = "dynamicParser";

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

    
    
    /// <summary>
    /// Build a dynamic parser class. then call BuildParser and returns a Parser instance ready to parse."/>
    /// </summary>
    /// <param name="model">the parser model built from parser description file</param>
    /// <returns>a Parser</returns>
    public (object parserBuildResult, Type parserType, Type lexerType) BuildParser(Model model)
    {
        DynamicParserName = model.ParserModel.Name;
        var lexerBuilder = new LexerBuilder(model.LexerModel.Name);
        var(enumType, extensionBuilder, assemblyBuilder, moduleBuilder) = lexerBuilder.BuildLexerEnum(model.LexerModel);
        
        EnumType = enumType;
        ObjectType = typeof(object);
        TokenType = BuilderHelper.BuildGenericType(typeof(Token<>),EnumType);
        TokenListType = BuilderHelper.BuildGenericType(typeof(List<>),TokenType);
        ObjectListType = BuilderHelper.BuildGenericType(typeof(List<>),ObjectType);
        GroupType = BuilderHelper.BuildGenericType(typeof(sly.parser.parser.Group<,>), EnumType, ObjectType);
        OptionGroupType = BuilderHelper.BuildGenericType(typeof(ValueOption<>), GroupType);
        OptionType = typeof(ValueOption<object>);
        GroupListType = BuilderHelper.BuildGenericType(typeof(List<>), GroupType);
        BuildExtensionType = typeof(Action<,,>).MakeGenericType(EnumType,typeof(LexemeAttribute),typeof(GenericLexer<>).MakeGenericType(enumType));
        LexerPostProcessType = typeof(LexerPostProcess<>).MakeGenericType(EnumType);

        TypeBuilder typeBuilder = moduleBuilder.DefineType(DynamicParserName, TypeAttributes.Public, typeof(object));
        AddParserRoot(typeBuilder,model.ParserModel.Root);
        AddParserOptimizations(typeBuilder, model.ParserModel);
        foreach (var rule in model.ParserModel.Rules)
        {
            BuildVisitor(typeBuilder,rule);
        }

        var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
        var il = constructorBuilder.GetILGenerator();        
        il.Emit(OpCodes.Ret);
        
        
        
        Type compiledType = typeBuilder.CreateType();
        return (BuildIt(compiledType, model?.ParserModel?.Root, extensionBuilder), compiledType, EnumType);
    }

    public void AddParserRoot(TypeBuilder builder, string root)
    {
        Type attributeType = typeof(ParserRootAttribute);
        
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[1] { typeof(string) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { root });

        builder.SetCustomAttribute(customAttributeBuilder);
    }
    
    public void AddParserOptimizations(TypeBuilder builder, ParserModel model)
    {
        if (model.UseMemoization)
        {
            Type attributeType = typeof(UseMemoizationAttribute);

            ConstructorInfo constructorInfo = attributeType.GetConstructor(new Type[] { });

            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { });

            builder.SetCustomAttribute(customAttributeBuilder);
        }
        if (model.BroadenTokenWindow)
        {
            Type attributeType = typeof(BroadenTokenWindowAttribute);

            ConstructorInfo constructorInfo = attributeType.GetConstructor(new Type[] { });

            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { });

            builder.SetCustomAttribute(customAttributeBuilder);
        }
        if (model.AutoCloseIndentations)
        {
            Type attributeType = typeof(AutoCloseIndentationsAttribute);

            ConstructorInfo constructorInfo = attributeType.GetConstructor(new Type[] { });

            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { });

            builder.SetCustomAttribute(customAttributeBuilder);
        }
    }
    
    public Result<Model> CompileModel(string modelSource, string parserName = "dynamicParser")
    {
        ParserBuilder<CLIToken, ICLIModel> builder = new ParserBuilder<CLIToken, ICLIModel>();
        var instance = new CLIParser();

        var buildParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        if (buildParser.IsOk)
        {
            var context = new ParserContext(parserName);
            var result = buildParser.Result.ParseWithContext(modelSource, context);
            if (result.IsError)
            {
                return result.Errors.Select(x => x.ErrorMessage).ToList();
            }
            else {
                Model model = result.Result as Model;
                return CheckModel(model);
            }
        }
        else
        {
            // should not happen
            return buildParser.Errors.Select(x => x.Message).ToList();
        }
    }

    public Result<Model> CheckModel( Result<Model> model)
    {
        bool hasRoot = model.result.ParserModel.Rules.Exists(x => x.IsRoot);
        if (!hasRoot)
        {
            model.AddError("model have root rule !");
        }

        var referencesVisitor = new ReferencesVisitor();
        ModelWalker<RuleReferences> referenceWalker = new ModelWalker<RuleReferences>(referencesVisitor);
        var references = referenceWalker.Walk(model, new RuleReferences());
        ;
        var referenceErrors = references.CheckReferences();
        
        model.AddErrors(referenceErrors);

        var lrc = new LeftRecursionChecker(model.result.ParserModel);
        var leftRecursions = lrc.CheckLeftRecursion();
        if (leftRecursions.foundRecursion)
        {
            foreach (var recursion in leftRecursions.recursions)
            {
                model.AddError($"found left recursion {string.Join(" > ",recursion)}.");
            }
        }
        
        
        return model;
    }
    
    
   
    
      public Result<List<(string format,string content)>,List<string>> Getz(string modelSource, string source, string parserName, List<(string format,SyntaxTreeProcessor processor)> processors, string rootRule = null)
    {
        var model = CompileModel(modelSource, parserName);
        
        if (model.IsError)
        {
            return model.error;
        }
        
        
        var buildResult = BuildParser(model);
        
        var parserType = typeof(Parser<,>).MakeGenericType(buildResult.lexerType,typeof(object));
        var buildResultType = typeof(BuildResult<>).MakeGenericType(parserType);
           
        
        //  return a list<string> if buildResult is error
        var isErrorResult = buildResultType.GetProperty("IsError").GetValue(buildResult.parserBuildResult, null) as bool?;
        if (isErrorResult.HasValue && isErrorResult.Value)
        {
            var errors = buildResultType.GetProperty("Errors").GetValue(buildResult.parserBuildResult, null) as
                List<InitializationError>;
            return errors.Select(x => x.Message).ToList();
        }
        
        var resultProperty = buildResultType.GetProperty("Result");
        var parser = resultProperty.GetValue(buildResult.parserBuildResult, null);

        var parseMethod = parserType.GetMethod("Parse", new[] { typeof(string), typeof(string) });
        var result = parseMethod.Invoke(parser, new object[] { source, null });

        var ParseResultType = typeof(ParseResult<,>).MakeGenericType(buildResult.lexerType, typeof(object));

        var x = ParseResultType.GetProperty("IsError").GetValue(result) as bool?;
        if (x.HasValue && x.Value)
        {
            var errors = ParseResultType.GetProperty("Errors").GetValue(result) as List<ParseError>;
            return errors.Select(x => x.ErrorMessage).ToList();
        }
        
        
        var parseResultProp = ParseResultType.GetProperty("SyntaxTree");
        var syntaxTree = parseResultProp.GetValue(result);


        
        
        if (processors != null && processors.Any())
        {
            List<(string format, string content)> results = new List<(string format, string content)>();
            foreach (var processor in processors)
            {
                var processed = processor.processor(buildResult.lexerType, parser.GetType(), syntaxTree);
                results.Add((processor.format,processed));
            }

            return results;
        }

        return null;
    }

    public static string SyntaxTreeToDotGraph(Type lexerType, Type parserType, object syntaxTree)
    {
        var graphvizType = typeof(GraphVizEBNFSyntaxTreeVisitor<>).MakeGenericType(lexerType);
        var visitor = graphvizType.GetConstructor(new Type[] { }).Invoke(new object[]{});
        
        var visited = graphvizType
            .GetMethod("VisitTree", new Type[] { syntaxTree.GetType() })
            .Invoke(visitor, new object[] { syntaxTree });

        var graph = graphvizType?
            .GetProperty("Graph")
            ?.GetValue(visitor);

        var dot = (graph as DotGraph);

        return dot.Compile();
    }

    public static string SyntaxTreeToJson(Type lexerType, Type parserTree, object syntaxTree)
    {
        var serialization = JsonConvert.SerializeObject(syntaxTree, Formatting.Indented);
        return serialization;
    }
    
    private object BuildIt(Type parserType, string root, Delegate extensionBuilder)
    {
        var constructor = parserType.GetConstructor(Type.EmptyTypes);
        var instance = constructor.Invoke(new object?[]{});
        var builderType = typeof(ParserBuilder<,>);
        builderType = builderType.MakeGenericType(EnumType, ObjectType);
        var builderconstructor =
            builderType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
        var builder = builderconstructor.Invoke(new object?[] { });
        var buildMethod = builderType.GetMethod("BuildParser", new Type[] { ObjectType,typeof(ParserType),typeof(string),BuildExtensionType,LexerPostProcessType});
        
        var x = buildMethod.Invoke(builder, new object?[] { instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, root,extensionBuilder,null }); 
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
        if (rule.IsPostfix && rule is PostfixRule postfix)
        {
            AddPostfix(builder, postfix);
        }
        if (rule.IsInfix && rule is InfixRule infix)
        {
            AddInfix(builder, infix);
        }
    }
    
    
    private void AddProduction(TypeBuilder builder, Rule rule)
    {

        var parameters = rule.Clauses.Select(x => BuildTypeParameter(x)).ToArray();
        
        var methodBuilder = AddMethod(builder, rule.Key, parameters);
        if (rule.IsOperand)
        {
            AddOperandAttribute(methodBuilder);
        }
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
    
    private int explicitPrefixCounter = 0;
    
    private void AddPrefix(TypeBuilder builder, PrefixRule prefix)
    {
        string name = prefix.Name;
        string operatorName = prefix.Name;
        if (prefix.IsExplicit)
        {
            name = explicitPrefixCounter.ToString();
            explicitPrefixCounter++;
            operatorName = $"'{operatorName}'";
        }

        var methodBuilder = AddMethod(builder, $"prefix_{name}", TokenType, ObjectType);
        
        Type attributeType = typeof(PrefixAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[3] { typeof(string),typeof(Associativity),typeof(int) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { operatorName,Associativity.Left,prefix.Precedence });

        methodBuilder.SetCustomAttribute(customAttributeBuilder);
    }

    private int explicitPostfixCounter = 0;
    
    private void AddPostfix(TypeBuilder builder, PostfixRule postfix)
    {
        
        string name = postfix.Name;
        string operatorName = postfix.Name;
        if (postfix.IsExplicit)
        {
            name = explicitPostfixCounter.ToString();
            explicitPostfixCounter++;
            operatorName = $"'{operatorName}'";
        }
        
        var methodBuilder = AddMethod(builder, $"postfix_{name}", ObjectType,TokenType);
        
        Type attributeType = typeof(PostfixAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[3] { typeof(string),typeof(Associativity),typeof(int) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { operatorName,Associativity.Left,postfix.Precedence });

        methodBuilder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private void AddInfix(TypeBuilder builder, InfixRule infix)
    {
        string operatorName = infix.Name;
        if (infix.IsExplicit)
        {
            operatorName = $"'{operatorName}'";
        }

        var methodBuilder = AddMethod(builder, $"infix_{infix.Name}_{infix.Associativity}_{infix.Precedence}",
            ObjectType, TokenType, ObjectType); 
        
        Type attributeType = typeof(InfixAttribute);
            
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[3] { typeof(string),typeof(Associativity),typeof(int) });
            
        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { operatorName, infix.Associativity, infix.Precedence });

        methodBuilder.SetCustomAttribute(customAttributeBuilder);
    }
    
    private static void AddOperandAttribute(MethodBuilder methodBuilder)
    {
        Type attributeType = typeof(OperandAttribute);
        ConstructorInfo constructorInfo = attributeType.GetConstructor(
            new Type[] { });

        CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
            constructorInfo, new object[] { });

        methodBuilder.SetCustomAttribute(customAttributeBuilder);
    }

    public MethodBuilder AddMethod(TypeBuilder builder, string name, params Type[] parameterTypes)
    {
        StringBuilder normalizedName = new StringBuilder();
        foreach (var c in name)
        {
            if (Char.IsDigit(c) || char.IsLetter(c))
            {
                normalizedName.Append(c);
            }
            else
            {
                normalizedName.Append('_');
            }
        }
        
        
        var methodBuilder = builder.DefineMethod(normalizedName.ToString(),
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
            case IndentClause indentClause:
            {
                return TokenType;
            } 
            case UIndentClause uIndentClause:
            {
                return TokenType;
            } 
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
                    case ChoiceClause c : 
                    {
                        if (c.IsTerminalChoice)
                        {
                            return TokenType;
                        }
                        else
                        {
                            return ObjectType;
                        }
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
                    case ChoiceClause c:
                    {
                        if (c.IsTerminalChoice)
                        {
                            return TokenListType;
                        }
                        return ObjectListType;
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
