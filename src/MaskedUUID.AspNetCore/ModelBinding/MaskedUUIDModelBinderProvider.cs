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
            var maskedUUIDService = context.Services.GetRequiredService<IMaskedUUIDService>();
            return new MaskedUUIDModelBinder(maskedUUIDService);
        }

        return null;
    }
}
