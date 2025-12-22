using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.Services;
using Microsoft.AspNetCore.Builder;

namespace MaskedUUID.AspNetCore.Extensions;

/// <summary>
/// IStartupFilter that initializes MaskedGuidConverter with the MaskedUUIDService
/// during application startup. This ensures the converter has access to the service
/// for encryption/decryption operations.
/// </summary>
internal class MaskedGuidConverterInitializationFilter : IStartupFilter
{
    private readonly IMaskedUUIDService _service;

    public MaskedGuidConverterInitializationFilter(IMaskedUUIDService service)
    {
        _service = service;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        // Initialize MaskedGuidConverter with the service
        // This must be called before the application starts handling requests
        MaskedGuidConverter.Initialize(_service);

        // Call the next filter
        return next;
    }
}
