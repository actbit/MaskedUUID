using MaskedUUID.AspNetCore.Types;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MaskedUUID.AspNetCore.OpenApi;

#if NET10_0_OR_GREATER
public sealed partial class MaskedGuidSchemaTransformer : IOpenApiSchemaTransformer
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

        schema.Type = Microsoft.OpenApi.JsonSchemaType.String;
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
#else
using Microsoft.OpenApi.Models;

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

        schema.Type = "string";
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
#endif
