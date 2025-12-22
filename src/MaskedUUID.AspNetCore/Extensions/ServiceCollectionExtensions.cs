using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.ModelBinding;
using MaskedUUID.AspNetCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace MaskedUUID.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaskedUUID(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            var maskedUUIDService = services.BuildServiceProvider()
                .GetRequiredService<IMaskedUUIDService>();

            options.JsonSerializerOptions.Converters.Add(
                new MaskedUUIDGuidConverter(maskedUUIDService));
            options.JsonSerializerOptions.Converters.Add(
                new MaskedUUIDNullableGuidConverter(maskedUUIDService));
        });

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
