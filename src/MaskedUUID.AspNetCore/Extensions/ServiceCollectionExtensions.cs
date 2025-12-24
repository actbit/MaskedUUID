using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.ModelBinding;
using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MaskedUUID.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMaskedUUID(this IServiceCollection services)
    {
        // Register service if not already registered
        // Note: IMaskedUUIDService is Scoped to work with Scoped IMaskedUUIDKeyProvider
        if (!services.Any(x => x.ServiceType == typeof(IMaskedUUIDService)))
        {
            services.AddScoped<IMaskedUUIDService, MaskedUUIDService>();
        }

        // Register JsonOptions configuration with MaskedGuid converter
        services.AddSingleton<IConfigureOptions<JsonOptions>, MaskedGuidJsonOptionsConfiguration>();

        // Register startup filter to initialize MaskedGuidConverter
        services.AddSingleton<IStartupFilter, MaskedGuidConverterInitializationFilter>();

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

    /// <summary>
    /// Configures OpenAPI schema for MaskedGuid type to display as string instead of exposing internal structure
    /// </summary>
    public static OpenApiOptions AddMaskedUUIDOpenApi(this OpenApiOptions options)
    {
        options.AddSchemaTransformer((schema, context, cancellationToken) =>
        {
            var type = context.JsonTypeInfo.Type;

            // Check if this is a MaskedGuid or nullable MaskedGuid
            if (type == typeof(MaskedGuid) || type == typeof(MaskedGuid?))
            {
                if (schema != null)
                {
                    // Completely replace with string schema (same as Guid)
                    schema.Type = JsonSchemaType.String;
                    schema.Format = "uuid";
                    schema.Description = "Masked UUID (obfuscated Guid)";
                    schema.Example = JsonNode.Parse("\"7a60cd35-b137-42a9-b6d8-648b62258609\"");

                    // Clear any object properties that might interfere
                    schema.Properties?.Clear();
                    schema.Required?.Clear();
                    schema.AllOf?.Clear();
                    schema.OneOf?.Clear();
                    schema.AnyOf?.Clear();
                    schema.Items = null;
                }
            }

            return Task.CompletedTask;
        });

        return options;
    }
}
