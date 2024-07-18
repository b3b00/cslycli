using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using clsy.cli.builder.checker;
using clsy.cli.model.tree.visitor;
using clsy.cli.model.tree.visitor.mermaid;
using csly.cli.model;
using csly.cli.model.parser;
using csly.cli.model.tree;
using csly.cli.parser;
using Newtonsoft.Json;
using sly.buildresult;
using LexerBuilder = clsy.cli.builder.lexer.LexerBuilder;
using sly.lexer;
using sly.lexer.fsm;
using sly.parser;
using sly.parser.generator;
using sly.parser.generator.visitor;
using sly.parser.parser;

namespace clsy.cli.builder.parser;

public delegate string SyntaxTreeProcessor(Type parserType, Type lexerType, ISyntaxNode tree);

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
    public (object parserBuildResult, Type parserType, Type lexerType) BuildParser(Model model, Chrono chrono = null)
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
        var result =  (BuildIt(compiledType, model?.ParserModel?.Root, extensionBuilder), compiledType, EnumType);
        if (chrono != null)
        {
            chrono.Tick("parser generation");   
        }

        return result;
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
    
    public Result<Model> CompileModel(string modelSource, string parserName = "dynamicParser", Chrono chrono = null)
    {
        ParserBuilder<CLIToken, ICLIModel> builder = new ParserBuilder<CLIToken, ICLIModel>();
        var instance = new CLIParser();

        if (chrono != null && !chrono.IsStarted)
        {
            chrono.Start();
        }
        
        var buildParser = builder.BuildParser(instance, ParserType.EBNF_LL_RECURSIVE_DESCENT, "root");
        
        if (buildParser.IsOk)
        {
            var context = new ParserContext(parserName);
            var result = buildParser.Result.ParseWithContext(modelSource, context);
            if (result.IsError)
            {
                if (chrono != null)
                {
                    chrono.Tick("model compilation");
                }
                return result.Errors.Select(x => x.ErrorMessage).ToList();
            }
            else {
                if (context.IsError)
                {
                    if (chrono != null)
                    {
                        chrono.Tick("model compilation");
                    }
                    return context.Errors;
                }
                Model model = result.Result as Model;

                var i = 0;
                var check = CheckModel(model);
                
                if (chrono != null)
                {
                    chrono.Tick("model compilation");
                }

                return check;
            }
        }
        else
        {
            if (chrono != null)
            {
                chrono.Tick("model compilation");
            }
            // should not happen
            return buildParser.Errors.Select(x => x.Message).ToList();
        }
    }

    public Result<Model> CheckModel( Result<Model> model)
    {
        bool hasRoot = model.result.ParserModel.Rules.Exists(x => x.IsRoot);
        if (!hasRoot)
        {
            model.AddError("model have no root rule !");
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

     

        var keywordDefinitionsByDefinition = model.result.LexerModel.Tokens
            .Where(x => x.Type == GenericToken.KeyWord)
            .GroupBy(x => x.Args[0]);
        foreach (var keyword in keywordDefinitionsByDefinition)
        {
            var count = keyword.Count();
            if (count > 1)
            {
                model.AddError($@"tokens {string.Join(", ",keyword.Select(x => x.Name))} define the same keyword ""{keyword.Key}""");
            }
        }
        
        var sugarDefinitionsByDefinition = model.result.LexerModel.Tokens
            .Where(x => x.Type == GenericToken.SugarToken)
            .GroupBy(x => x.Args[0]);
        foreach (var sugar in sugarDefinitionsByDefinition)
        {
            var count = sugar.Count();
            if (count > 1)
            {
                model.AddError($@"tokens {string.Join(", ",sugar.Select(x => x.Name))} define the same sugar token ""{sugar.Key}""");
            }
        }
        
        var extensionTokensVisitor = new ExtensionTokensVisitor();
        var extensionTokensWalker = new ModelWalker<ExtensionTokenChecks>(extensionTokensVisitor);
        var extensionTokensChecks = extensionTokensWalker.Walk(model, new ExtensionTokenChecks(true));
        if (extensionTokensChecks.IsError)
        {
            model.AddErrors(extensionTokensChecks.Errors);
        }
        
        
        
        return model;
    }



    public Result<string> Compile(string grammar, string parserName = "dynamicParser", Chrono chrono = null)
    {
        var model = CompileModel(grammar, parserName,chrono);
        
        if (model.IsError)
        {
            return model.error;
        }
        
        
        var buildResult = BuildParser(model, chrono);
        
        var parserType = typeof(Parser<,>).MakeGenericType(buildResult.lexerType,typeof(object));
        var buildResultType = typeof(BuildResult<>).MakeGenericType(parserType);
           
        
        //  return a list<string> if buildResult is error
        var isErrorResult = buildResultType.GetProperty("IsError").GetValue(buildResult.parserBuildResult, null) as bool?;
        if (isErrorResult.HasValue && isErrorResult.Value)
        {
            var errors = buildResultType.GetProperty("Errors").GetValue(buildResult.parserBuildResult, null) as
                List<InitializationError>;
            return errors.Select(x => $"[{x.Code}] {x.Message}").ToList();
        }
        
        var resultProperty = buildResultType.GetProperty("Result");
        var parser = resultProperty.GetValue(buildResult.parserBuildResult, null);

        return "OK";
    }
    
       public Result<string> Parse(string modelSource, string source, string parserName, string rootRule = null, Chrono chrono = null)
    {
        var model = CompileModel(modelSource, parserName,chrono);
        
        if (model.IsError)
        {
            return model.error;
        }
        
        
        var buildResult = BuildParser(model, chrono);
        
        var parserType = typeof(Parser<,>).MakeGenericType(buildResult.lexerType,typeof(object));
        var buildResultType = typeof(BuildResult<>).MakeGenericType(parserType);
           
        
        //  return a list<string> if buildResult is error
        var isErrorResult = buildResultType.GetProperty("IsError").GetValue(buildResult.parserBuildResult, null) as bool?;
        if (isErrorResult.HasValue && isErrorResult.Value)
        {
            var errors = buildResultType.GetProperty("Errors").GetValue(buildResult.parserBuildResult, null) as
                List<InitializationError>;
            return errors.Select(x => $"[{x.Code}] {x.Message}").ToList();
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

        return "OK";
        
    }

    public Result<List<(string format, string content)>, List<string>> Getz(string modelSource, string source,
        string parserName, List<(string format, SyntaxTreeProcessor processor)> processors, string rootRule = null,
        Chrono chrono = null)
    {
        var model = CompileModel(modelSource, parserName);
        
        if (model.IsError)
        {
            return model.error;
        }
        
        
        var buildResult = BuildParser(model, chrono);
        
        var parserType = typeof(Parser<,>).MakeGenericType(buildResult.lexerType,typeof(object));
        var buildResultType = typeof(BuildResult<>).MakeGenericType(parserType);
           
        
        //  return a list<string> if buildResult is error
        var isErrorResult = buildResultType.GetProperty("IsError").GetValue(buildResult.parserBuildResult, null) as bool?;
        if (isErrorResult.HasValue && isErrorResult.Value)
        {
            var errors = buildResultType.GetProperty("Errors").GetValue(buildResult.parserBuildResult, null) as
                List<InitializationError>;
            return errors.Select(x => $"[{x.Code}] {x.Message}").ToList();
        }
        
        var resultProperty = buildResultType.GetProperty("Result");
        var parser = resultProperty.GetValue(buildResult.parserBuildResult, null);

        var parseMethod = parserType.GetMethod("Parse", new[] { typeof(string), typeof(string) });
        var result = parseMethod.Invoke(parser, new object[] { source, null });
        if (chrono != null)
        {
         chrono.Tick("source parsing");   
        }

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
                var untyperType = typeof(TreeUntyper<>).MakeGenericType(buildResult.lexerType);
                
                var iSyntaxNodeType = typeof(sly.parser.syntax.tree.ISyntaxNode<>).MakeGenericType(buildResult.lexerType);
                var untypeMethod = untyperType.GetMethod("Untype", new[] { iSyntaxNodeType });
                var untyped = untypeMethod.Invoke(null, new object[] { syntaxTree });
                var tree = untyped as ISyntaxNode;
                
                var processed = processor.processor(buildResult.lexerType, parser.GetType(), tree);
                results.Add((processor.format,processed));
            }

            if (chrono != null)
            {
                chrono.Tick($"syntax tree processing {string.Join(", ", processors.Select(x => x.format))}");
            }

            return results;
        }

        return null;
    }

    public Result<ISyntaxNode> GetSyntaxTree(string modelSource, string source,
        string parserName, string rootRule = null,
        Chrono chrono = null)
    {
        var model = CompileModel(modelSource, parserName);

        if (model.IsError)
        {
            return model.error;
        }


        var buildResult = BuildParser(model, chrono);

        var parserType = typeof(Parser<,>).MakeGenericType(buildResult.lexerType, typeof(object));
        var buildResultType = typeof(BuildResult<>).MakeGenericType(parserType);


        //  return a list<string> if buildResult is error
        var isErrorResult =
            buildResultType.GetProperty("IsError").GetValue(buildResult.parserBuildResult, null) as bool?;
        if (isErrorResult.HasValue && isErrorResult.Value)
        {
            var errors = buildResultType.GetProperty("Errors").GetValue(buildResult.parserBuildResult, null) as
                List<InitializationError>;
            return errors.Select(x => $"[{x.Code}] {x.Message}").ToList();
        }

        var resultProperty = buildResultType.GetProperty("Result");
        var parser = resultProperty.GetValue(buildResult.parserBuildResult, null);

        var parseMethod = parserType.GetMethod("Parse", new[] { typeof(string), typeof(string) });
        var result = parseMethod.Invoke(parser, new object[] { source, null });
        if (chrono != null)
        {
            chrono.Tick("source parsing");
        }

        var ParseResultType = typeof(ParseResult<,>).MakeGenericType(buildResult.lexerType, typeof(object));

        var x = ParseResultType.GetProperty("IsError").GetValue(result) as bool?;
        if (x.HasValue && x.Value)
        {
            var errors = ParseResultType.GetProperty("Errors").GetValue(result) as List<ParseError>;
            return errors.Select(x => x.ErrorMessage).ToList();
        }


        var parseResultProp = ParseResultType.GetProperty("SyntaxTree");
        var syntaxTree = parseResultProp.GetValue(result);


        if (chrono != null)
        {
            chrono.Tick("get syntax tree");
        }

        var untyperType = typeof(TreeUntyper<>).MakeGenericType(buildResult.lexerType);

        var iSyntaxNodeType = typeof(sly.parser.syntax.tree.ISyntaxNode<>).MakeGenericType(buildResult.lexerType);
        var untypeMethod = untyperType.GetMethod("Untype", new[] { iSyntaxNodeType });
        var untyped = untypeMethod.Invoke(null, new object[] { syntaxTree });
        var tree = untyped as ISyntaxNode;

        if (tree != null)
        {
            return new Result<ISyntaxNode>(tree);

        }

        if (chrono != null)
        {
            chrono.Tick("un type syntax tree");
            chrono.Stop();
        }

        if (tree != null)
        {
            return new Result<ISyntaxNode>(tree);
        }


        return null;
    }




    public static string SyntaxTreeToDotGraph(Type lexerType, Type parserType, ISyntaxNode syntaxTree)
    {
        var visitor = new GraphVizEBNFSyntaxTreeVisitor();
        visitor.VisitTree(syntaxTree);
        var graph = visitor.Graph;
        return graph.Compile();
    }
    
    public static string SyntaxTreeToMermaid(Type lexerType, Type parserType, object syntaxTree)
    {
        var graphvizType = typeof(MermaidEBNFSyntaxTreeVisitor<>).MakeGenericType(lexerType);
        var visitor = graphvizType.GetConstructor(new Type[] { }).Invoke(new object[]{});
        
        var visited = graphvizType
            .GetMethod("VisitTree", new Type[] { syntaxTree.GetType() })
            .Invoke(visitor, new object[] { syntaxTree });

        var graph = graphvizType?
            .GetProperty("Graph")
            ?.GetValue(visitor);

        var mermaid = (graph as MermaidGraph);

        return mermaid.Compile();
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
        if (rule is ManyPrefixRule prefixes)
        {
            AddPrefixes(builder, prefixes);
        }
        if (rule.IsPostfix && rule is PostfixRule postfix)
        {
            AddPostfix(builder, postfix);
        }
        if (rule is ManyPostfixRule postfixes)
        {
            AddPostfixes(builder, postfixes);
        }
        if (rule.IsInfix && rule is InfixRule infix)
        {
            AddInfix(builder, infix);
        }

        if (rule is ManyInfixRule infixes)
        {
            AddInfixes(builder, infixes);
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
        _AddNodeNameAttribute(methodBuilder, rule);
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

    private void _AddNodeNameAttribute(MethodBuilder builder, Rule rule)
    {
        if (rule.TryGetNodeName(out var nodeName))
        {
            Type attributeType = typeof(NodeNameAttribute);
        
            ConstructorInfo constructorInfo = attributeType.GetConstructor(
                new Type[1] { typeof(string) });
            
            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { nodeName });

            builder.SetCustomAttribute(customAttributeBuilder);
        }
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
        _AddNodeNameAttribute(methodBuilder, prefix);
    }

    private void AddPrefixes(TypeBuilder builder, ManyPrefixRule prefixes)
    {
        string name = prefixes.GetName(ref explicitPrefixCounter);
        
        var methodBuilder = AddMethod(builder, $"prefix_{name}", TokenType, ObjectType);
        
        Type attributeType = typeof(PrefixAttribute);

        foreach (var prefix in prefixes.Prefixes)
        {
            string operatorName = prefix.Name;
            if (prefix.IsExplicit)
            {
                explicitPrefixCounter++;
                operatorName = $"'{operatorName}'";
            }


            ConstructorInfo constructorInfo = attributeType.GetConstructor(
                new Type[3] { typeof(string), typeof(Associativity), typeof(int) });

            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { operatorName, Associativity.Left, prefix.Precedence });

            methodBuilder.SetCustomAttribute(customAttributeBuilder);
            _AddNodeNameAttribute(methodBuilder, prefixes);
        }
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
        _AddNodeNameAttribute(methodBuilder, postfix);
    }
    
    private void AddPostfixes(TypeBuilder builder, ManyPostfixRule postfixes)
    {
        string name = postfixes.GetName(ref explicitPostfixCounter);
        
        var methodBuilder = AddMethod(builder, name, ObjectType, TokenType);
        
        Type attributeType = typeof(PostfixAttribute);

        foreach (var postfix in postfixes.Postfixes)
        {
            string operatorName = postfix.Name;
            if (postfix.IsExplicit)
            {
                explicitPostfixCounter ++;
                operatorName = $"'{operatorName}'";
            }

            ConstructorInfo constructorInfo = attributeType.GetConstructor(
                new Type[3] { typeof(string), typeof(Associativity), typeof(int) });

            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { operatorName, Associativity.Left, postfix.Precedence });

            methodBuilder.SetCustomAttribute(customAttributeBuilder);
            _AddNodeNameAttribute(methodBuilder, postfixes);
        }
    }
    
    private int explicitInfixCounter = 0;
    
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
        _AddNodeNameAttribute(methodBuilder, infix);
    }
    
    private void AddInfixes(TypeBuilder builder, ManyInfixRule infixes)
    {
        string name = infixes.GetName(ref explicitInfixCounter);
        
        var methodBuilder = AddMethod(builder, $"infix_{name}",
            ObjectType, TokenType, ObjectType); 
        
        Type attributeType = typeof(InfixAttribute);

        foreach (var infix in infixes.Infixes)
        {
            string operatorName = infix.Name;
            if (infix.IsExplicit)
            {
                explicitPostfixCounter ++;
                operatorName = $"'{operatorName}'";
            }

            ConstructorInfo constructorInfo = attributeType.GetConstructor(
                new Type[3] { typeof(string), typeof(Associativity), typeof(int) });

            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { operatorName, infix.Associativity, infix.Precedence });

            methodBuilder.SetCustomAttribute(customAttributeBuilder);
            _AddNodeNameAttribute(methodBuilder, infixes);
        }
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
