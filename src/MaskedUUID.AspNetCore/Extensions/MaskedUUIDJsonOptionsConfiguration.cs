using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MaskedUUID.AspNetCore.Extensions;

/// <summary>
/// Factory for IConfigureOptions that creates converters with lazy-loaded service.
/// This avoids calling BuildServiceProvider() during configuration.
/// </summary>
internal class MaskedUUIDJsonOptionsConfiguration : IConfigureOptions<JsonOptions>
{
    private readonly IServiceProvider _serviceProvider;
    private IMaskedUUIDService? _cachedService;

    public MaskedUUIDJsonOptionsConfiguration(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Configure(JsonOptions options)
    {
        var service = GetService();
        options.JsonSerializerOptions.Converters.Add(
            new MaskedUUIDGuidConverter(service));
        options.JsonSerializerOptions.Converters.Add(
            new MaskedUUIDNullableGuidConverter(service));
    }

    private IMaskedUUIDService GetService()
    {
        if (_cachedService != null)
            return _cachedService;

        _cachedService = _serviceProvider.GetRequiredService<IMaskedUUIDService>();
        return _cachedService;
    }
}
