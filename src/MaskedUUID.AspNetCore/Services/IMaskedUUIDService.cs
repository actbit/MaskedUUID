namespace MaskedUUID.AspNetCore.Services;

public interface IMaskedUUIDService
{
    Task<string> EncodeAsync(Guid guid);
    Task<Guid> DecodeAsync(string maskedUuid);
    Task<List<string>> EncodeListAsync(IEnumerable<Guid> guids);
    Task<List<Guid>> DecodeListAsync(IEnumerable<string> maskedUuids);
    bool IsValidMaskedUUID(string value);
    string EncodeSynchronous(Guid guid);
    Guid DecodeSynchronous(string maskedUuid);
}
