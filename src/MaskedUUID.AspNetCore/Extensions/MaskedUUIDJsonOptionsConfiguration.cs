using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaskedUUID.AspNetCore.Extensions;

/// <summary>
/// Configures JSON serialization options with MaskedGuid converter.
/// MaskedGuid is a dedicated type for properties that should be masked,
/// ensuring type-safe and automatic conversion without attribute checks.
/// </summary>
internal class MaskedGuidJsonOptionsConfiguration : IConfigureOptions<JsonOptions>
{
    public void Configure(JsonOptions options)
    {
        // Register MaskedGuid converter globally
        // MaskedGuid type automatically uses this converter
        // No attribute checking needed - type determines behavior
        options.JsonSerializerOptions.Converters.Add(new MaskedGuidConverter());
    }
}
