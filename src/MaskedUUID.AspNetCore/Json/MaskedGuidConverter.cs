using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.Types;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskedUUID.AspNetCore.Json;

/// <summary>
/// Converter for MaskedGuid properties
/// Automatically encrypts/decrypts GUID values using MaskedUUID encoding
/// </summary>
public class MaskedGuidConverter : JsonConverter<MaskedGuid>
{
    private static IMaskedUUIDService? _service;

    public MaskedGuidConverter()
    {
    }

    /// <summary>
    /// Initialize the converter with the MaskedUUID service
    /// Must be called during application startup
    /// </summary>
    public static void Initialize(IMaskedUUIDService service)
    {
        _service = service;
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
            var service = _service ?? throw new InvalidOperationException(
                "MaskedGuidConverter not initialized. Call MaskedGuidConverter.Initialize(service) during app startup.");
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
            var service = _service ?? throw new InvalidOperationException(
                "MaskedGuidConverter not initialized. Call MaskedGuidConverter.Initialize(service) during app startup.");
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
}
