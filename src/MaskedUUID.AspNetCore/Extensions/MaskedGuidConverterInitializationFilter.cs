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
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // Create a scope to resolve the scoped IMaskedUUIDService
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IMaskedUUIDService>();
                // Initialize MaskedGuidConverter with the service
                // This must be called before the application starts handling requests
                MaskedGuidConverter.Initialize(service);
            }

            // Call the next filter
            next(app);
        };
    }
}
