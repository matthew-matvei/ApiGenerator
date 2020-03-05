using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiGenerator
{
    public static class CodeGenerator
    {
        public static CodeCompileUnit BuildSchemaCode(
            Schema schema)
        {
            var unit = new CodeCompileUnit();

            var methods = schema.Actions.Select(ToMethod);

            var controller = new CodeTypeDeclaration(schema.Name);
            methods.ToList().ForEach(m => controller.Members.Add(m));
            controller.BaseTypes.Add(new CodeTypeReference("ControllerBase"));

            var @namespace = new CodeNamespace("MyWebApi.Controllers");
            @namespace.Types.Add(controller);
            @namespace.Imports.Add(new CodeNamespaceImport("Microsoft.AspNetCore.Mvc"));

            unit.Namespaces.Add(@namespace);

            return unit;
        }

        public static void GenerateCode(
            CodeCompileUnit unit,
            string schemaName)
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var fileName = Path.Join(
                GeneratedControllersFolder,
                $"{schemaName}.{provider.FileExtension}"
            );

            using var textWriter = new IndentedTextWriter(
                new StreamWriter(fileName, append: false),
                "   "
            );

            provider.GenerateCodeFromCompileUnit(
                unit,
                textWriter,
                new CodeGeneratorOptions()
            );

            textWriter.Close();
        }

        private static string GeneratedControllersFolder => 
            Path.Join(
                Assembly.GetExecutingAssembly().CodeBase,
                "..",
                "..",
                "..",
                "..",
                "src",
                "MyWebApi",
                "Controllers"
            );

        private static CodeMemberMethod ToMethod(SchemaAction action)
        {
            CodeParameterDeclarationExpression toParameter(
                SchemaActionParameter parameter)
                {
                    var parameterExpression = new CodeParameterDeclarationExpression
                    {
                        Type = new CodeTypeReference(parameter.Type),
                        Name = parameter.Name
                    };

                    var attribute = new CodeAttributeDeclaration(
                        parameter.Source == "body"
                            ? "FromBody"
                            : "FromQuery"
                    );

                    parameterExpression
                        .CustomAttributes
                        .Add(attribute);

                    return parameterExpression;
                }
                
            var member = new CodeMemberMethod
            {
                Name = action.Name,
                Attributes = MemberAttributes.Public
            };

            var httpMethodAttribute = new CodeAttributeDeclaration(
                action.Method == "post"
                    ? "HttpPost"
                    : "HttpGet",
                new CodeAttributeArgument(
                    new CodePrimitiveExpression(action.Path)
                )
            );

            member.CustomAttributes.Add(httpMethodAttribute);
            member.ImplementationTypes.Add("ApiController");
            
            action
                .Parameters
                .Select(toParameter)
                .ToList()
                .ForEach(p => member.Parameters.Add(p));

            return member;
        }
    }
}