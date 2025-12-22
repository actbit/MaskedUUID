using MaskedUUID.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MaskedUUID.AspNetCore.ModelBinding;

public class MaskedUUIDModelBinder : IModelBinder
{
    private readonly IMaskedUUIDService _service;

    public MaskedUUIDModelBinder(IMaskedUUIDService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
            throw new ArgumentNullException(nameof(bindingContext));

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
            return Task.CompletedTask;

        try
        {
            var guid = _service.DecodeSynchronous(value);
            bindingContext.Result = ModelBindingResult.Success(guid);
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
        }

        return Task.CompletedTask;
    }
}
