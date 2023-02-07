using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json.Serialization;
using clsy.cli.builder.parser.cli.model;
using csly.cli.model.lexer;
using sly.lexer;
using sly.lexer.fsm;

namespace clsy.cli.builder.lexer;

public class LexerBuilder
{

        public static string DynamicAssemblyName = "dynamicAssembly";

        public string DynamicLexerName { get; set; } = "DynamicLexer";

        public LexerBuilder(string name)
        {
            DynamicLexerName = name;
        }
        
    
        public (Type enumType, Delegate extensionBuilder, AssemblyBuilder assembly, ModuleBuilder moduleBuilder) BuildLexerEnum(LexerModel model)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

            AssemblyName aName = new AssemblyName(DynamicAssemblyName);

            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(aName,
                AssemblyBuilderAccess.Run);

            ModuleBuilder moduleBuilder = dynamicAssembly.DefineDynamicModule(aName.Name);

            EnumBuilder enumBuilder = moduleBuilder.DefineEnum(DynamicLexerName, TypeAttributes.Public, typeof(int));

            int i = 0;
            
            enumBuilder.DefineLiteral($"{model.Name.ToUpper()}_EOS", i);
            i++;

            foreach (var tokenModel in model.TokensByName )
            {
                var enumValueBuilder = enumBuilder.DefineLiteral(tokenModel.Key, i);

                AddAttributes(tokenModel.Value, enumValueBuilder);
    
                
                i++;
            }

            Type finished = enumBuilder.CreateType();

            var extensionBuilder = BuildExtensionIfNeeded(model, finished);

            return (finished,extensionBuilder,dynamicAssembly,moduleBuilder);
        }

        private Delegate BuildExtensionIfNeeded(LexerModel model, Type? enumType)
        {
           
            Func<Func<FSMMatch<GenericToken>, FSMMatch<GenericToken>>, NodeCallback<GenericToken>> ConvertLambdaCallbackToDelegate = func =>
            {
                return new NodeCallback<GenericToken>(func);
            };

            var extensions = model.Tokens.Where(x => x.Type == GenericToken.Extension).Cast<ExtensionTokenModel>().ToList();

            IList<Expression> ifs = new List<Expression>();

            var tokenParameter = Expression.Parameter(enumType);
            var lexemeParameter = Expression.Parameter(typeof(LexemeAttribute));
            var lexerType = typeof(GenericLexer<>).MakeGenericType(enumType);
            var lexerParameter = Expression.Parameter(lexerType);
            var fsmbuildertype = typeof(FSMLexerBuilder<GenericToken>);
            var fsmmatchType = typeof(FSMMatch<GenericToken>);
            var fsmmatchParameter = Expression.Parameter(fsmmatchType);

            
            var b = Expression.Property(lexerParameter, "FSMBuilder");
            
            foreach (var extensionTokenModel in extensions)
            {
                var e = Expression.Constant(enumType);
                var n = Expression.Constant(extensionTokenModel.Name);
                
                var tostr = enumType.GetMethod("ToString", new Type[]{});
                var eq = Expression.Equal(n, Expression.Call(tokenParameter,tostr));
                
                var extensionBuilder = BuildFsmForExtension(extensionTokenModel);
                var lambdaBuild = Expression.Invoke(Expression.Constant(extensionBuilder), b);
                
                // .End(Extension)
                var endMethod = fsmbuildertype.GetMethod("End", new Type[] {typeof(GenericToken), typeof(bool) });
                var end = Expression.Call(b, endMethod,Expression.Constant(GenericToken.Extension),Expression.Constant(false));
                
                // TODO : add callback

                var getProps = Expression.Property(fsmmatchParameter, "Properties");
                var addMethod = typeof(Dictionary<,>).MakeGenericType(typeof(string), typeof(object)).GetMethod("Add",new []{typeof(string),typeof(object)});

                
                // note  : the needed cast (Convert(type(object)). otherwise Expression fails to build as it thinks that the generated enum is not assignable from Object ! 
                var assign = Expression.Call(getProps, addMethod,Expression.Constant(GenericLexer<GenericToken>.DerivedToken),
                    Expression.Convert(tokenParameter,typeof(object)));
                
                var callbackBody = Expression.Block(assign, fsmmatchParameter);

                var callbackLambda = Expression.Lambda(callbackBody, false, fsmmatchParameter);
                var nodecallbacktype = typeof(NodeCallback<GenericToken>);
                var callback = Expression.Invoke(Expression.Constant(ConvertLambdaCallbackToDelegate), callbackLambda);
                
                var callbackMethod = fsmbuildertype.GetMethod("CallBack", new Type[] {nodecallbacktype});
                var setCallBack = Expression.Call(b, callbackMethod, callback);

                // Build the ifthenelse block
                
                var thenBlock = Expression.Block(lambdaBuild,end,setCallBack);
                
                var ifthenelse = Expression.IfThen(eq, thenBlock);
                ifs.Add(ifthenelse);
            }

            // TODO : build a lambda with all ifs
            var block = Expression.Block(ifs);
            var l = Expression.Lambda(block, false, tokenParameter, lexemeParameter, lexerParameter);
            var d = l.Compile();
            
            // TODO convert to BuildExtension
            // System.Action<ExtensionLexer,sly.lexer.LexemeAttribute,sly.lexer.GenericLexer<ExtensionLexer>>
            //     sly.lexer.fsm.BuildExtension<ExtensionLexer>
            
            return d as Delegate;
            
            
        }

        private Action<FSMLexerBuilder<GenericToken>> BuildFsmForExtension(ExtensionTokenModel token)
        {
            Action<FSMLexerBuilder<GenericToken>> fmsTransitioner = builder =>
            {
                builder = builder.GoTo(GenericLexer<GenericToken>.start);
                foreach (var transition in token.Transitions)
                {
                    builder = Transition(builder, transition);
                }
                
            };
            return fmsTransitioner;
        }

        private FSMLexerBuilder<GenericToken> Repeat(FSMLexerBuilder<GenericToken> builder, Func<FSMLexerBuilder<GenericToken> , string , FSMLexerBuilder<GenericToken>> DoTransition,
            ITransition transition)
        {
            if (transition.Repeater != null)
            {
                switch (transition.Repeater.RepeaterType)
                {
                    case RepeaterType.Count:
                    {
                        for (int i = 0; i < transition.Repeater.Count; i++)
                        {
                            builder = DoTransition(builder, null);
                        }

                        break;
                    }
                    case RepeaterType.ZeroOrMore:
                    {
                        string loopingNode = Guid.NewGuid().ToString();
                        builder.Mark(loopingNode);
                        builder = DoTransition(builder, loopingNode);
                        break;
                    }
                    case RepeaterType.OneOrMore:
                    {
                        string loopingNode = Guid.NewGuid().ToString();
                        builder = DoTransition(builder, null)
                            .Mark(loopingNode);
                        builder = DoTransition(builder, loopingNode);

                        break;
                    }
                    case RepeaterType.Option:
                    {
                        throw new NotImplementedException("optional transitions are not yet implemented.");
                        break;
                    }
                }
            }
            else
            {
                builder = DoTransition(builder, null);
            }
            return builder;
        }
        
        private FSMLexerBuilder<GenericToken> Transition(FSMLexerBuilder<GenericToken> builder , ITransition transition)
        {
            Func<FSMLexerBuilder<GenericToken>, string, FSMLexerBuilder<GenericToken>> DoTransition = null;
            
            if (transition is CharacterTransition charTransition)
            {
                DoTransition = (lexerBuilder, toNode) =>
                {
                    if (string.IsNullOrEmpty(toNode))
                    {
                        lexerBuilder.Transition(charTransition.Character);
                    }
                    else
                    {
                        lexerBuilder.TransitionTo(charTransition.Character,toNode);
                    }
                    return lexerBuilder;
                };

            }
            else if (transition is RangeTransition rangeTransition)
            {
                DoTransition = (lexerBuilder, toNode) =>
                {
                    if (string.IsNullOrEmpty(toNode))
                    {
                        builder.MultiRangeTransition(rangeTransition.Ranges
                            .Select(x => (x.StartCharacter, x.EndCharacter))
                            .ToArray());
                    }
                    else
                    {
                        lexerBuilder.MultiRangeTransitionTo(toNode,rangeTransition.Ranges.Select(x => (x.StartCharacter, x.EndCharacter))
                            .ToArray());
                    }
                    return lexerBuilder;
                };
            }
            else if (transition is ExceptTransition exceptTransition)
            {
                throw new NotImplementedException("except transitions are not yet implemented.");
            }

            builder = Repeat(builder, DoTransition, transition);

            return builder;
        }




        private void AddAttributes(List<TokenModel> models, FieldBuilder builder)
        {
            foreach (var model in models)
            {
                Add(model.Type, model.IdentifierType,builder, model.Args);    
            }
        }

        private void Add(GenericToken genericToken, IdentifierType identifierType, FieldBuilder builder,
            params string[] args)
        {
            if (genericToken == GenericToken.Comment)
            {
                bool isSingleLine = args.Length == 1;
                Type attributeType = isSingleLine ? typeof(SingleLineCommentAttribute) : typeof(MultiLineCommentAttribute);

                if (isSingleLine)
                {
                    ConstructorInfo constructorInfo = attributeType.GetConstructor(
                        new Type[3] { typeof(string), typeof(bool), typeof(int) });

                    CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                        constructorInfo, new object[] { args[0],false,Channels.Comments });

                    builder.SetCustomAttribute(customAttributeBuilder);
                }
                else
                {
                    ConstructorInfo constructorInfo = attributeType.GetConstructor(
                        new Type[4] { typeof(string), typeof(string), typeof(bool), typeof(int) });

                    CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                        constructorInfo, new object[] { args[0],args[1],false,Channels.Comments  });

                    builder.SetCustomAttribute(customAttributeBuilder);
                }
               
                AddJsonAttribute(builder);
            }
            else if (genericToken == GenericToken.Identifier)
            {
                var attributeType = identifierType switch
                {
                    IdentifierType.Alpha => typeof(AlphaIdAttribute),
                    IdentifierType.AlphaNumeric => typeof(AlphaNumIdAttribute),
                    IdentifierType.AlphaNumericDash => typeof(AlphaNumDashIdAttribute),
                    _ => typeof(AlphaIdAttribute)
                };
                ConstructorInfo constructorInfo = attributeType.GetConstructor(
                    new Type[0] { });

                CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                    constructorInfo, new object[] { });

                builder.SetCustomAttribute(customAttributeBuilder);
                
                AddJsonAttribute(builder);
            }
            else if (genericToken == GenericToken.Extension)
            {
                var attributeType = typeof(ExtensionAttribute);
                ConstructorInfo constructorInfo = attributeType.GetConstructor(
                    new Type[0] { });
                CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                    constructorInfo, new object[] { });
                builder.SetCustomAttribute(customAttributeBuilder);
                
                AddJsonAttribute(builder);
            }
            else 
            {
                Type attributeType = typeof(LexemeAttribute);

                ConstructorInfo constructorInfo = attributeType.GetConstructor(
                    new Type[2] { typeof(GenericToken), typeof(string[]) });

                CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                    constructorInfo, new object[] { genericToken, args });

                builder.SetCustomAttribute(customAttributeBuilder);

                AddJsonAttribute(builder);
            }
        }

        private static void AddJsonAttribute(FieldBuilder builder)
        {
            Type attributeType;
            attributeType = typeof(JsonConverterAttribute);
            var enumConverterType = typeof(JsonStringEnumConverter);
            var serialConstructorInfo = attributeType.GetConstructor(
                new Type[1] { typeof(Type) });
            var serialAttributeBuilder = new CustomAttributeBuilder(
                serialConstructorInfo, new object[] { enumConverterType });
            builder.SetCustomAttribute(serialAttributeBuilder);
        }


        
}