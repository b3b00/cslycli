using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json.Serialization;
using clsy.cli.builder.parser.cli.model;
using csly.cli.model.lexer;
using sly.lexer;
using sly.lexer.fsm;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace clsy.cli.builder.lexer;

public class LexerBuilder
{

        public static string DynamicAssemblyName = "dynamicAssembly";

        public string DynamicLexerName { get; set; } = "DynamicLexer";

        public LexerBuilder(string name)
        {
            DynamicLexerName = name;
        }
        
        public (object lexerBuildResult, Type tokenType) BuildLexer(LexerModel model)
        {
            DynamicLexerName = model.Name;

// Create the type and save the assembly.
            var finished = BuildLexerEnum(model);
                
                return (BuildIt(finished.enumType), finished.enumType);
        }
        
        public (Type enumType, AssemblyBuilder assembly, ModuleBuilder moduleBuilder) BuildLexerEnum(LexerModel model)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;

// Create a dynamic assembly in the current application domain,
// and allow it to be executed and saved to disk.

            AssemblyName aName = new AssemblyName(DynamicAssemblyName);

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
                
            return (finished,dynamicAssembly,moduleBuilder);
        }

        private void AddAttribute(TokenModel model, FieldBuilder builder)
        {
            Add(model.Type,builder, model.Args);
        }

        private void Add(GenericToken genericToken, FieldBuilder builder, params string[] args)
        {
            Type attributeType = typeof(LexemeAttribute);
            
            ConstructorInfo constructorInfo = attributeType.GetConstructor(
                new Type[2] { typeof(GenericToken), typeof(string[]) });
            
            CustomAttributeBuilder customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { genericToken, args });

            builder.SetCustomAttribute(customAttributeBuilder);
            
            attributeType = typeof(JsonConverterAttribute);
            var enumConverterType = typeof(JsonStringEnumConverter);
            constructorInfo = attributeType.GetConstructor(
                new Type[1] { typeof(Type)});
            customAttributeBuilder = new CustomAttributeBuilder(
                constructorInfo, new object[] { enumConverterType });
            
            builder.SetCustomAttribute(customAttributeBuilder);
        }


        private object BuildIt(Type enumType)
        {
            var methods = typeof(sly.lexer.LexerBuilder).GetMethods().ToList();
            var builders = methods.Where(x => x.Name.Contains("Build")).Where(x => x.GetParameters().Length == 2).ToList();
            
            MethodInfo method = typeof(sly.lexer.LexerBuilder).GetMethod("BuildLexer", new Type[] {typeof(BuildExtension<>),typeof(LexerPostProcess<>)});
            method = builders[0];
            MethodInfo genericMethod = method.MakeGenericMethod(enumType);
            var built = genericMethod.Invoke(null, new object[] {null,null});
            return built;
        }
        
}