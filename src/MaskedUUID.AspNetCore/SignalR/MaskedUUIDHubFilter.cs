using Microsoft.AspNetCore.SignalR;

namespace MaskedUUID.AspNetCore.SignalR;

/// <summary>
/// Hub filter that tracks the current Hub invocation scope for MaskedUUID.
/// This filter sets the AsyncLocal scope before each Hub method invocation
/// and clears it after completion.
/// </summary>
public class MaskedUUIDHubFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        // Set the current scope for this Hub invocation
        MaskedUUIDHubScopeProvider.SetScope(invocationContext.ServiceProvider);

        try
        {
            return await next(invocationContext);
        }
        finally
        {
            // Clear the scope after invocation
            MaskedUUIDHubScopeProvider.SetScope(null);
        }
    }

    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        // Set scope for OnConnected
        MaskedUUIDHubScopeProvider.SetScope(context.ServiceProvider);

        try
        {
            await next(context);
        }
        finally
        {
            MaskedUUIDHubScopeProvider.SetScope(null);
        }
    }

    public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
    {
        // Set scope for OnDisconnected
        MaskedUUIDHubScopeProvider.SetScope(context.ServiceProvider);

        try
        {
            await next(context, exception);
        }
        finally
        {
            MaskedUUIDHubScopeProvider.SetScope(null);
        }
    }
}
