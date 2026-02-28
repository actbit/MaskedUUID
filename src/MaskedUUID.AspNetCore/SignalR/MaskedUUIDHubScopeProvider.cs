using Microsoft.AspNetCore.SignalR;

namespace MaskedUUID.AspNetCore.SignalR;

/// <summary>
/// Provides access to the current SignalR Hub scope via AsyncLocal.
/// This allows the MaskedGuidSignalRConverter to resolve services within the Hub's scope.
/// </summary>
public static class MaskedUUIDHubScopeProvider
{
    private static readonly AsyncLocal<IServiceProvider?> _currentScope = new();

    /// <summary>
    /// Gets the service provider for the current Hub invocation scope.
    /// </summary>
    public static IServiceProvider? CurrentScope => _currentScope.Value;

    /// <summary>
    /// Sets the current scope. Should only be called by MaskedUUIDHubFilter.
    /// </summary>
    internal static void SetScope(IServiceProvider? serviceProvider)
    {
        _currentScope.Value = serviceProvider;
    }
}
