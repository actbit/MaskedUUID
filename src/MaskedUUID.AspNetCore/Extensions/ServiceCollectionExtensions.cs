using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.ModelBinding;
using MaskedUUID.AspNetCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace MaskedUUID.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaskedUUID(this IServiceCollection services)
    {
        // Register service if not already registered
        if (!services.Any(x => x.ServiceType == typeof(IMaskedUUIDService)))
        {
            services.AddScoped<IMaskedUUIDService, MaskedUUIDService>();
        }

        // Register JsonOptions configuration using IConfigureOptions<T>
        // This uses constructor injection, avoiding BuildServiceProvider() call
        services.AddSingleton<IConfigureOptions<JsonOptions>, MaskedUUIDJsonOptionsConfiguration>();

        return services;
    }

    public static IMvcBuilder AddMaskedUUIDModelBinder(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddMvcOptions(options =>
        {
            options.ModelBinderProviders.Insert(0, new MaskedUUIDModelBinderProvider());
        });

        return mvcBuilder;
    }
}
