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
        
        public (Type enumType, AssemblyBuilder assembly, ModuleBuilder moduleBuilder) BuildLexerEnum(LexerModel model)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

            AssemblyName aName = new AssemblyName(DynamicAssemblyName);

            var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(aName,
                AssemblyBuilderAccess.Run);

            ModuleBuilder moduleBuilder = dynamicAssembly.DefineDynamicModule(aName.Name);

            EnumBuilder enumBuilder = moduleBuilder.DefineEnum(DynamicLexerName, TypeAttributes.Public, typeof(int));

            int i = 0;

            foreach (var tokenModel in model.TokensByName )
            {
                var enumValueBuilder = enumBuilder.DefineLiteral(tokenModel.Key, i);

                AddAttributes(tokenModel.Value, enumValueBuilder);
    
                
                i++;
            }

            Type finished = enumBuilder.CreateType();

            BuildExtensionIfNeeded(model);

            return (finished,dynamicAssembly,moduleBuilder);
        }

        private void BuildExtensionIfNeeded(LexerModel model)
        {
            var extensions = model.Tokens.Where(x => x.Type == GenericToken.Extension).Cast<ExtensionTokenModel>().ToList();
            foreach (var extensionTokenModel in extensions)
            {
                BuildFsmForExtension(extensionTokenModel);
            }
        }

        private void BuildFsmForExtension(ExtensionTokenModel token)
        {
            Action<FSMLexerBuilder<GenericToken>> action = builder =>
            {
                builder = builder.GoTo(GenericLexer<GenericToken>.start);
                foreach (var transition in token.Transitions)
                {
                    builder = Transition(builder, transition);
                }
            };
        }

        private FSMLexerBuilder<GenericToken> Repeat(FSMLexerBuilder<GenericToken> builder, Func<FSMLexerBuilder<GenericToken> , string , FSMLexerBuilder<GenericToken>> DoTransition,
            ITransition transition)
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