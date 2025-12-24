using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.Types;

namespace MaskedUUID.AspNetCore.ModelBinding;

/// <summary>
/// ModelBinder provider that handles MaskedGuid type parameters for URL/query parameters
/// </summary>
public class MaskedUUIDModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var modelType = context.Metadata.ModelType;
        var isMaskedGuidType = modelType == typeof(MaskedGuid) || modelType == typeof(MaskedGuid?);

        // Support MaskedGuid type directly
        if (isMaskedGuidType)
        {
            // Get HttpContextAccessor from root provider
            var httpContextAccessor = context.Services.GetRequiredService<IHttpContextAccessor>();

            // Create a factory function that resolves the service from the request scope
            return new MaskedUUIDModelBinder(httpContextAccessor);
        }

        return null;
    }
}
