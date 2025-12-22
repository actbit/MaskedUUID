using MaskedUUID.AspNetCore.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaskedUUID.AspNetCore.Json;

public class MaskedUUIDGuidConverter : JsonConverter<Guid>
{
    private readonly IMaskedUUIDService _service;

    public MaskedUUIDGuidConverter(IMaskedUUIDService service)
    {
        _service = service;
    }

    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return Guid.Empty;

        var maskedUuid = reader.GetString();
        if (string.IsNullOrEmpty(maskedUuid))
            return Guid.Empty;

        try
        {
            return _service.DecodeSynchronous(maskedUuid);
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to decode MaskedUUID", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        if (value == Guid.Empty)
        {
            writer.WriteNullValue();
            return;
        }

        try
        {
            var maskedUuid = _service.EncodeSynchronous(value);
            writer.WriteStringValue(maskedUuid);
        }
        catch (Exception ex)
        {
            throw new JsonException("Failed to encode GUID to MaskedUUID", ex);
        }
    }
}
