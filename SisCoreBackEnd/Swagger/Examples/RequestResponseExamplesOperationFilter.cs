using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace SisCoreBackEnd.Swagger.Examples
{
    internal sealed class RequestResponseExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var declaringType = context.MethodInfo.DeclaringType?.FullName;
            if (string.IsNullOrWhiteSpace(declaringType))
            {
                return;
            }

            var methodKey = $"{declaringType}.{context.MethodInfo.Name}".ToLowerInvariant();

            if (!SwaggerExamplesCatalog.TryGetExamples(methodKey, out var definition))
            {
                return;
            }

            if (definition.RequestExample != null &&
                operation.RequestBody?.Content.TryGetValue("application/json", out var requestMediaType) == true)
            {
                requestMediaType.Example = SerializeToOpenApiAny(definition.RequestExample);
            }
            else if (definition.RequestExample != null && operation.Parameters?.Count > 0)
            {
                using var document = SerializeToJsonElement(definition.RequestExample);
                var element = document.RootElement;
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var parameter in operation.Parameters)
                    {
                        if (element.TryGetProperty(parameter.Name, out var property))
                        {
                            parameter.Example = property.ToOpenApiAny();
                        }
                    }
                }
            }

            if (definition.Responses.Count > 0)
            {
                foreach (var (statusCode, example) in definition.Responses)
                {
                    var statusKey = statusCode.ToString();
                    if (!operation.Responses.TryGetValue(statusKey, out var response) || example is null)
                    {
                        continue;
                    }

                    if (response.Content.TryGetValue("application/json", out var responseMediaType))
                    {
                        responseMediaType.Example = SerializeToOpenApiAny(example);
                    }
                }
            }
        }

        private static IOpenApiAny SerializeToOpenApiAny(object example)
        {
            using var document = SerializeToJsonElement(example);
            return document.RootElement.ToOpenApiAny();
        }

        private static JsonDocument SerializeToJsonElement(object example)
        {
            var json = JsonSerializer.Serialize(example, SerializerOptions);
            return JsonDocument.Parse(json);
        }

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }
}


