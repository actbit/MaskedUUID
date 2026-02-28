using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace MaskedUUID.AspNetCore.ModelBinding;

/// <summary>
/// ModelBinder that decodes masked UUIDs for MaskedGuid parameters only.
/// Supports MaskedGuid and MaskedGuid? type parameters.
/// </summary>
public class MaskedUUIDModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var service = bindingContext.HttpContext?.RequestServices.GetService<IMaskedUUIDService>();
        if (service is null)
        {
            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                "IMaskedUUIDService is not available. Ensure it is registered and the request has a valid HttpContext.");
            return Task.CompletedTask;
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
            return Task.CompletedTask;

        try
        {
            var guid = service.DecodeSynchronous(value);

            // Support MaskedGuid parameter types
            if (bindingContext.ModelType == typeof(MaskedGuid))
            {
                bindingContext.Result = ModelBindingResult.Success(new MaskedGuid(guid));
            }
            else if (bindingContext.ModelType == typeof(MaskedGuid?))
            {
                if (guid == Guid.Empty)
                {
                    bindingContext.Result = ModelBindingResult.Success((MaskedGuid?)null);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success((MaskedGuid?)new MaskedGuid(guid));
                }
            }
            else
            {
                // Default: Guid
                bindingContext.Result = ModelBindingResult.Success(guid);
            }
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
        }

        return Task.CompletedTask;
    }
}
