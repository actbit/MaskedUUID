using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.Types;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MaskedUUID.AspNetCore.Json;

/// <summary>
/// Converter for MaskedGuid properties
/// Automatically encrypts/decrypts GUID values using MaskedUUID encoding
/// </summary>
public class MaskedGuidConverter : JsonConverter<MaskedGuid>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MaskedGuidConverter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override MaskedGuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return new MaskedGuid(Guid.Empty);

        var maskedUuid = reader.GetString();
        if (string.IsNullOrEmpty(maskedUuid))
            return new MaskedGuid(Guid.Empty);

        try
        {
            var service = ResolveService();
            var guid = service.DecodeSynchronous(maskedUuid);
            return new MaskedGuid(guid);
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to decode MaskedUUID", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, MaskedGuid value, JsonSerializerOptions options)
    {
        if (value.Value == Guid.Empty)
        {
            writer.WriteNullValue();
            return;
        }

        try
        {
            var service = ResolveService();
            var maskedUuid = service.EncodeSynchronous(value.Value);
            writer.WriteStringValue(maskedUuid);
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to encode GUID to MaskedUUID", ex);
        }
    }

    private IMaskedUUIDService ResolveService()
    {
        var requestServices = _httpContextAccessor.HttpContext?.RequestServices;
        if (requestServices is not null)
        {
            return requestServices.GetRequiredService<IMaskedUUIDService>();
        }

        throw new InvalidOperationException(
            "No HttpContext available to resolve IMaskedUUIDService. Ensure MaskedGuid is used within an HTTP request scope.");
    }
}
