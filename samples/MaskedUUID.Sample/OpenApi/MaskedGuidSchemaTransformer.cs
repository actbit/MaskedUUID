using MaskedUUID.AspNetCore.Types;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MaskedUUID.Sample.OpenApi;

public sealed class MaskedGuidSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo?.Type;
        if (type == null)
        {
            return Task.CompletedTask;
        }

        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying != typeof(MaskedGuid))
        {
            return Task.CompletedTask;
        }

        schema.Type = JsonSchemaType.String;
        schema.Format = "uuid";
        schema.Properties?.Clear();
        schema.Required?.Clear();
        schema.AllOf?.Clear();
        schema.AnyOf?.Clear();
        schema.OneOf?.Clear();
        schema.Items = null;
        schema.AdditionalPropertiesAllowed = false;

        return Task.CompletedTask;
    }
}
