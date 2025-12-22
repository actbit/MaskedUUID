namespace MaskedUUID.AspNetCore.KeyProviders;

public interface IMaskedUUIDKeyProvider
{
    Task<(ulong K0, ulong K1)> GetKeysAsync();

    (ulong K0, ulong K1) GetKeysSynchronous();
}
