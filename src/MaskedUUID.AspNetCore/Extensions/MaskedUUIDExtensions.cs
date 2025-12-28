using UUIDv47Sharp;

namespace MaskedUUID.AspNetCore.Extensions;

internal static class MaskedUUIDExtensions
{
    public static string ToMaskedUUID(this Guid guid, ulong k0, ulong k1)
    {
        var key = new Key(k0, k1);
        var uuid = guid.ToUuidStable();
        var masked = Uuid47Codec.Encode(uuid, key);
        return masked.ToString();
    }

    public static Guid FromMaskedUUID(this string maskedUuid, ulong k0, ulong k1)
    {
        var key = new Key(k0, k1);
        var masked = Uuid.Parse(maskedUuid);
        var original = Uuid47Codec.Decode(masked, key);
        return original.ToGuidStable();
    }

    public static List<string> ToMaskedUUIDList(this IEnumerable<Guid> guids, ulong k0, ulong k1)
    {
        return guids.Select(g => g.ToMaskedUUID(k0, k1)).ToList();
    }

    public static List<Guid> FromMaskedUUIDList(this IEnumerable<string> maskedUuids, ulong k0, ulong k1)
    {
        return maskedUuids.Select(m => m.FromMaskedUUID(k0, k1)).ToList();
    }
}
