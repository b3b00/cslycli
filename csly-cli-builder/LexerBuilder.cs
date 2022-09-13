﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using csly_cli_model;
using sly.lexer;
using sly.lexer.fsm;

namespace csly_cli_builder;

public class LexerBuilder
{

        public static string DynamixAssemblyName = "LexerAssembly";

        public static string DynamicLexerName = "DynamicLexer";

        public static (object lexerBuildResult, Type tokenType) BuildLexer(LexerModel model)
        {
                AppDomain currentDomain = AppDomain.CurrentDomain;

// Create a dynamic assembly in the current application domain,
// and allow it to be executed and saved to disk.

                AssemblyName aName = new AssemblyName(DynamixAssemblyName);

                var dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(aName,
                        AssemblyBuilderAccess.Run);


// Define a dynamic module in "TempAssembly" assembly. For a single-
// module assembly, the module has the same name as the assembly.
                ModuleBuilder moduleBuilder = dynamicAssembly.DefineDynamicModule(aName.Name);

// Define a public enumeration with the name "Elevation" and an 
// underlying type of Integer.
                EnumBuilder enumBuilder = moduleBuilder.DefineEnum(DynamicLexerName, TypeAttributes.Public, typeof(int));

// Define two members, "High" and "Low".

            int i = 0;

            foreach (var tokenModel in model.Tokens )
            {
                var enumValueBuilder = enumBuilder.DefineLiteral(tokenModel.Name, i);

                AddAttribute(tokenModel, enumValueBuilder);

                
                i++;
            }


// Create the type and save the assembly.
                Type finished = enumBuilder.CreateType();
                
                return (BuildIt(finished), finished);
        }

        private static void AddAttribute(TokenModel model, FieldBuilder builder)
        {
            switch (model.Type) 
            {
                case GenericToken.Int:
                case GenericToken.Double:
                {
                    Add(model.Type,builder, new string[] {});
                    break;
                }
                case GenericToken.String:
                {
                    Add(model.Type,builder,"\"","\\");
                    break;
                }
                default:
                {
                    Add(model.Type,builder, new []{model.Arg});
                    break;
                }
            }

            

            
            
        }

        private static void Add(GenericToken genericToken, FieldBuilder builder, params string[] args)
        {
            Type attributeType = typeof(LexemeAttribute);
            
            ConstructorInfo constructorInfo = attributeType.GetConstructor(
                new Type[2] { typeof(GenericToken), typeof(string[]) });
            
            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { genericToken, args });

            builder.SetCustomAttribute(customAttributeBuilder);
        }


        private static object BuildIt(Type enumType)
        {
            var methods = typeof(sly.lexer.LexerBuilder).GetMethods().ToList();
            var builders = methods.Where(x => x.Name.Contains("Build")).Where(x => x.GetParameters().Length == 2).ToList();
            
            MethodInfo method = typeof(sly.lexer.LexerBuilder).GetMethod("BuildLexer", new Type[] {typeof(BuildExtension<>),typeof(LexerPostProcess<>)});
            method = builders[0];
            MethodInfo genericMethod = method.MakeGenericMethod(enumType);
            var built = genericMethod.Invoke(null, new object[] {null,null});
            return built;
        }
        
        private static Type GetGenericType(Type type, Type enumType)
        {
            return type.MakeGenericType(enumType);
        }
}