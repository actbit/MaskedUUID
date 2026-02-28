using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.KeyProviders;
using MaskedUUID.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MaskedUUID.AspNetCore.Extensions;

/// <summary>
/// SignalR builder extensions for MaskedUUID support
/// </summary>
public static class SignalRBuilderExtensions
{
    /// <summary>
    /// Adds MaskedUUID support to SignalR by registering the MaskedGuid converter and Hub filter.
    /// Note: You must register IMaskedUUIDKeyProvider before calling this method.
    /// </summary>
    /// <param name="builder">The SignalR builder.</param>
    /// <returns>The SignalR builder for chaining.</returns>
    /// <remarks>
    /// This method adds:
    /// 1. MaskedUUIDHubFilter - Tracks Hub invocation scope for proper scoped service resolution
    /// 2. MaskedGuidSignalRConverter - Converts MaskedGuid values using the current scope
    ///
    /// Usage:
    /// <code>
    /// services.AddScoped&lt;IMaskedUUIDKeyProvider, MyKeyProvider&gt;();
    /// services.AddSignalR().AddMaskedUUID();
    /// </code>
    ///
    /// For KeyProvider lifetime:
    /// - Singleton: Works without any special handling
    /// - Scoped: Properly resolved within Hub invocation scope via the filter
    /// </remarks>
    public static ISignalRBuilder AddMaskedUUID(this ISignalRBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        // Register IMaskedUUIDService if not already registered
        if (!builder.Services.Any(x => x.ServiceType == typeof(Services.IMaskedUUIDService)))
        {
            builder.Services.AddScoped<Services.IMaskedUUIDService, Services.MaskedUUIDService>();
        }

        // Register the Hub filter to track Hub invocation scope
        builder.Services.AddSingleton<IHubFilter, MaskedUUIDHubFilter>();

        // Use IConfigureOptions pattern to properly inject dependencies
        builder.Services.AddSingleton<IConfigureOptions<JsonHubProtocolOptions>, MaskedGuidSignalRJsonOptionsConfiguration>();

        return builder;
    }

    /// <summary>
    /// Adds MaskedUUID support to SignalR with reference keys (for development/testing).
    /// Uses ReferenceUUIDv47KeyProvider as the default key provider.
    /// </summary>
    /// <param name="builder">The SignalR builder.</param>
    /// <returns>The SignalR builder for chaining.</returns>
    /// <remarks>
    /// Warning: Do not use this in production. Use AddMaskedUUID() with your own key provider instead.
    /// </remarks>
    public static ISignalRBuilder AddMaskedUUIDWithReferenceKeys(this ISignalRBuilder builder)
    {
        // Register IMaskedUUIDKeyProvider if not already registered
        if (!builder.Services.Any(x => x.ServiceType == typeof(IMaskedUUIDKeyProvider)))
        {
            builder.Services.AddScoped<IMaskedUUIDKeyProvider, ReferenceUUIDv47KeyProvider>();
        }

        return builder.AddMaskedUUID();
    }

    /// <summary>
    /// Adds MaskedUUID support to SignalR with a pre-configured IHttpContextAccessor.
    /// Use this method when you need more control over dependency resolution.
    /// Note: This overload does not register the Hub filter, so scoped KeyProviders
    /// will create a new scope per serialization operation.
    /// </summary>
    /// <param name="builder">The SignalR builder.</param>
    /// <param name="httpContextAccessor">The HttpContextAccessor instance.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <returns>The SignalR builder for chaining.</returns>
    public static ISignalRBuilder AddMaskedUUID(
        this ISignalRBuilder builder,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        // Register IMaskedUUIDService if not already registered
        if (!builder.Services.Any(x => x.ServiceType == typeof(Services.IMaskedUUIDService)))
        {
            builder.Services.AddScoped<Services.IMaskedUUIDService, Services.MaskedUUIDService>();
        }

        builder.AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.Converters.Add(
                new MaskedGuidSignalRConverter(serviceProvider, httpContextAccessor));
        });

        return builder;
    }
}
