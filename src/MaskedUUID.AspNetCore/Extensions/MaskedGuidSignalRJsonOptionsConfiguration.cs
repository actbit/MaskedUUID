using MaskedUUID.AspNetCore.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace MaskedUUID.AspNetCore.Extensions;

/// <summary>
/// Configures SignalR JSON serialization options with MaskedGuid converter.
/// This configuration properly handles scoped services for both HTTP and WebSocket connections.
/// </summary>
internal class MaskedGuidSignalRJsonOptionsConfiguration : IConfigureOptions<JsonHubProtocolOptions>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public MaskedGuidSignalRJsonOptionsConfiguration(
        IServiceProvider serviceProvider,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Configure(JsonHubProtocolOptions options)
    {
        options.PayloadSerializerOptions.Converters.Add(
            new MaskedGuidSignalRConverter(_serviceProvider, _httpContextAccessor));
    }
}
