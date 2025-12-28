using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.KeyProviders;
using MaskedUUID.AspNetCore.ModelBinding;
using MaskedUUID.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace MaskedUUID.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaskedUUID(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        // Register service if not already registered
        if (!services.Any(x => x.ServiceType == typeof(IMaskedUUIDService)))
        {
            services.AddScoped<IMaskedUUIDService, MaskedUUIDService>();
        }

        // Register JsonOptions configuration with MaskedGuid converter
        services.AddSingleton<IConfigureOptions<JsonOptions>, MaskedGuidJsonOptionsConfiguration>();

        return services;
    }

    public static IServiceCollection AddMaskedUUIDWithReferenceKeys(this IServiceCollection services)
    {
        if (!services.Any(x => x.ServiceType == typeof(IMaskedUUIDKeyProvider)))
        {
            services.AddScoped<IMaskedUUIDKeyProvider, ReferenceUUIDv47KeyProvider>();
        }

        return services.AddMaskedUUID();
    }

    public static IMvcBuilder AddMaskedUUIDModelBinder(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddMvcOptions(options =>
        {
            options.ModelBinderProviders.Insert(0, new MaskedUUIDModelBinderProvider());
        });

        return mvcBuilder;
    }

    public static IMvcBuilder AddMaskedUUIDControllers(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.AddMaskedUUID();
        return mvcBuilder.AddMaskedUUIDModelBinder();
    }
}
