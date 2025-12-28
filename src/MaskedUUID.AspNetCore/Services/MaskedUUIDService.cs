using MaskedUUID.AspNetCore.Extensions;
using MaskedUUID.AspNetCore.KeyProviders;
using Microsoft.Extensions.Logging;

namespace MaskedUUID.AspNetCore.Services;

public class MaskedUUIDService : IMaskedUUIDService
{
    private readonly IMaskedUUIDKeyProvider _keyProvider;
    private readonly ILogger<MaskedUUIDService> _logger;

    public MaskedUUIDService(
        IMaskedUUIDKeyProvider keyProvider,
        ILogger<MaskedUUIDService> logger)
    {
        _keyProvider = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));
        _logger = logger;
    }

    public async Task<string> EncodeAsync(Guid guid)
    {
        try
        {
            if (guid == Guid.Empty)
                throw new ArgumentException("Guid cannot be empty", nameof(guid));

            var (keyK0, keyK1) = await _keyProvider.GetKeysAsync();
            return guid.ToMaskedUUID(keyK0, keyK1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encoding GUID to MaskedUUID: {Guid}", guid);
            throw;
        }
    }

    public async Task<Guid> DecodeAsync(string maskedUuid)
    {
        try
        {
            if (string.IsNullOrEmpty(maskedUuid))
                throw new ArgumentException("MaskedUUID string cannot be null or empty", nameof(maskedUuid));

            var (keyK0, keyK1) = await _keyProvider.GetKeysAsync();
            return maskedUuid.FromMaskedUUID(keyK0, keyK1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding MaskedUUID string to GUID: {MaskedUUID}", maskedUuid);
            throw;
        }
    }

    public async Task<List<string>> EncodeListAsync(IEnumerable<Guid> guids)
    {
        if (guids is null)
            throw new ArgumentNullException(nameof(guids));

        var (keyK0, keyK1) = await _keyProvider.GetKeysAsync();
        return guids.ToMaskedUUIDList(keyK0, keyK1);
    }

    public async Task<List<Guid>> DecodeListAsync(IEnumerable<string> maskedUuids)
    {
        if (maskedUuids is null)
            throw new ArgumentNullException(nameof(maskedUuids));

        var (keyK0, keyK1) = await _keyProvider.GetKeysAsync();
        return maskedUuids.FromMaskedUUIDList(keyK0, keyK1);
    }

    public string EncodeSynchronous(Guid guid)
    {
        try
        {
            if (guid == Guid.Empty)
                throw new ArgumentException("Guid cannot be empty", nameof(guid));

            var (keyK0, keyK1) = _keyProvider.GetKeysSynchronous();
            return guid.ToMaskedUUID(keyK0, keyK1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encoding GUID to MaskedUUID (sync): {Guid}", guid);
            throw;
        }
    }

    public Guid DecodeSynchronous(string maskedUuid)
    {
        try
        {
            if (string.IsNullOrEmpty(maskedUuid))
                throw new ArgumentException("MaskedUUID string cannot be null or empty", nameof(maskedUuid));

            var (keyK0, keyK1) = _keyProvider.GetKeysSynchronous();
            return maskedUuid.FromMaskedUUID(keyK0, keyK1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decoding MaskedUUID string to GUID (sync): {MaskedUUID}", maskedUuid);
            throw;
        }
    }
}
