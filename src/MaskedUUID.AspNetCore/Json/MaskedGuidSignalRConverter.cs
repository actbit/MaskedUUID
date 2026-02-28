using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.SignalR;
using MaskedUUID.AspNetCore.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskedUUID.AspNetCore.Json;

/// <summary>
/// SignalR-specific converter for MaskedGuid properties.
/// Supports multiple scope resolution strategies:
/// 1. Hub scope (via MaskedUUIDHubScopeProvider) - Preferred for SignalR
/// 2. HttpContext.RequestServices - For HTTP-based connections
/// 3. Temporary scope creation - Fallback for other scenarios
/// </summary>
/// <remarks>
/// For proper Hub scope resolution, ensure MaskedUUIDHubFilter is registered:
/// <code>
/// services.AddSignalR().AddMaskedUUID();
/// </code>
///
/// For KeyProvider implementations:
/// - Singleton KeyProvider: No special handling needed
/// - Scoped KeyProvider: Will be resolved within the Hub's scope when using the filter
/// </remarks>
public class MaskedGuidSignalRConverter : JsonConverter<MaskedGuid>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly IServiceProvider _rootServiceProvider;

    /// <summary>
    /// Creates a new instance of MaskedGuidSignalRConverter.
    /// </summary>
    /// <param name="serviceProvider">The root service provider for fallback resolution.</param>
    /// <param name="httpContextAccessor">Optional HttpContextAccessor for HTTP-based scenarios.</param>
    public MaskedGuidSignalRConverter(
        IServiceProvider serviceProvider,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _rootServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _httpContextAccessor = httpContextAccessor;
    }

    public override MaskedGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new MaskedGuid(Guid.Empty);

        var maskedUuid = reader.GetString();
        if (string.IsNullOrEmpty(maskedUuid))
            return new MaskedGuid(Guid.Empty);

        return ResolveWithService(service => service.DecodeSynchronous(maskedUuid))
            .Then(guid => new MaskedGuid(guid));
    }

    public override void Write(Utf8JsonWriter writer, MaskedGuid value, JsonSerializerOptions options)
    {
        if (value.Value == Guid.Empty)
        {
            writer.WriteNullValue();
            return;
        }

        var maskedUuid = ResolveWithService(service => service.EncodeSynchronous(value.Value));
        writer.WriteStringValue(maskedUuid);
    }

    /// <summary>
    /// Resolves the service and executes the operation using the most appropriate scope.
    /// Priority: Hub Scope > HttpContext Scope > Temporary Scope
    /// </summary>
    private T ResolveWithService<T>(Func<IMaskedUUIDService, T> operation)
    {
        // 1. Try Hub scope first (set by MaskedUUIDHubFilter)
        var hubScope = MaskedUUIDHubScopeProvider.CurrentScope;
        if (hubScope is not null)
        {
            var service = hubScope.GetRequiredService<IMaskedUUIDService>();
            return operation(service);
        }

        // 2. Try HttpContext scope (HTTP-based SignalR connections)
        var requestServices = _httpContextAccessor?.HttpContext?.RequestServices;
        if (requestServices is not null)
        {
            var service = requestServices.GetRequiredService<IMaskedUUIDService>();
            return operation(service);
        }

        // 3. Fallback: Create a temporary scope
        // This handles edge cases where neither Hub nor HttpContext scope is available
        using var scope = _rootServiceProvider.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IMaskedUUIDService>();
        return operation(scopedService);
    }
}

/// <summary>
/// Extension methods for functional-style operations.
/// </summary>
internal static class FunctionalExtensions
{
    public static TResult Then<T, TResult>(this T value, Func<T, TResult> func) => func(value);
}
