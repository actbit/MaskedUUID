using MaskedUUID.AspNetCore.OpenApi;
using Microsoft.AspNetCore.OpenApi;

namespace MaskedUUID.AspNetCore.Extensions;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions AddMaskedGuidSchemaTransformer(this OpenApiOptions options)
    {
        options.AddSchemaTransformer<MaskedGuidSchemaTransformer>();
        return options;
    }
}
