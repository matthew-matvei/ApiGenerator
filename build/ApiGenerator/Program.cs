using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;

namespace ApiGenerator
{
    public static class Program
    {
        public static void Main()
        {
            GetSchemas()
                .Select(ParseSchema)
                .Select(ValidateSchema)
                .Select(schema => (
                    unit: CodeGenerator.BuildSchemaCode(schema),
                    schemaName: schema.Name))
                .ToList()
                .ForEach(args => 
                    CodeGenerator.GenerateCode(args.unit, args.schemaName));
        }

        private static IEnumerable<Stream> GetSchemas()
        {
            var schemasFolder = Path.Join(
                Directory.GetCurrentDirectory(),
                "ApiSchemas");
                
            return Directory
                .EnumerateFiles(schemasFolder)
                .Select(File.OpenRead);
        }

        private static Schema ParseSchema(
            Stream schemaStream)
        {
            using var reader = new StreamReader(
                schemaStream, 
                Encoding.UTF8);

            var text = reader.ReadToEnd();
            reader.Close();
            return JsonSerializer.Deserialize<Schema>(text);
        }

        private static Schema ValidateSchema(
            Schema schema)
        {
            var validationErrors = new List<string>();
            if (!string.IsNullOrWhiteSpace(schema.Name))
                validationErrors.Add($"The schema's {nameof(schema.Name)} cannot be empty");

            if (validationErrors.Any())
            {
                throw new InvalidOperationException(
                    string.Join(Environment.NewLine, validationErrors)
                );
            }

            return schema;
        }
    }
}
