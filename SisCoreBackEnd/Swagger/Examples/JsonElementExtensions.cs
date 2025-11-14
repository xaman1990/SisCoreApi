using Microsoft.OpenApi.Any;
using System.Text.Json;

namespace SisCoreBackEnd.Swagger.Examples
{
    internal static class JsonElementExtensions
    {
        public static IOpenApiAny ToOpenApiAny(this JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => CreateObject(element),
                JsonValueKind.Array => CreateArray(element),
                JsonValueKind.String => new OpenApiString(element.GetString() ?? string.Empty),
                JsonValueKind.Number => element.TryGetInt64(out var int64)
                    ? new OpenApiLong(int64)
                    : element.TryGetDouble(out var dbl)
                        ? new OpenApiDouble(dbl)
                        : new OpenApiString(element.GetRawText()),
                JsonValueKind.True => new OpenApiBoolean(true),
                JsonValueKind.False => new OpenApiBoolean(false),
                JsonValueKind.Null => new OpenApiNull(),
                JsonValueKind.Undefined => new OpenApiNull(),
                _ => new OpenApiString(element.GetRawText())
            };
        }

        private static IOpenApiAny CreateObject(JsonElement element)
        {
            var openApiObject = new OpenApiObject();
            foreach (var property in element.EnumerateObject())
            {
                openApiObject[property.Name] = property.Value.ToOpenApiAny();
            }
            return openApiObject;
        }

        private static IOpenApiAny CreateArray(JsonElement element)
        {
            var openApiArray = new OpenApiArray();
            foreach (var item in element.EnumerateArray())
            {
                openApiArray.Add(item.ToOpenApiAny());
            }
            return openApiArray;
        }
    }
}


