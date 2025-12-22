using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using MaskedUUID.AspNetCore.Attributes;
using MaskedUUID.AspNetCore.Services;

namespace MaskedUUID.AspNetCore.ModelBinding;

public class MaskedUUIDModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.Metadata.ModelType != typeof(Guid) && context.Metadata.ModelType != typeof(Guid?))
        {
            return null;
        }

        var hasMaskedUUIDAttribute = false;

        if (context.Metadata.ValidatorMetadata != null)
        {
            hasMaskedUUIDAttribute = context.Metadata.ValidatorMetadata
                .OfType<MaskedUUIDAttribute>()
                .Any();
        }

        if (!hasMaskedUUIDAttribute)
        {
            return null;
        }

        var maskedUUIDService = context.Services.GetRequiredService<IMaskedUUIDService>();
        return new MaskedUUIDModelBinder(maskedUUIDService);
    }
}
