using UUIDv47Sharp;

namespace MaskedUUID.AspNetCore.Extensions;

public static class GuidUuidWorkarounds
{
    public static Uuid ToUuidStable(this Guid guid)
    {
        // Workaround for Guid->Uuid endianness mismatch in UUIDv47Sharp.ToUuid().
        Span<byte> bytes = stackalloc byte[16];
        if (!guid.TryWriteBytes(bytes, bigEndian: true, out var written) || written != 16)
        {
            throw new InvalidOperationException("Failed to write Guid bytes.");
        }
        return new Uuid(bytes);
    }

    public static Guid ToGuidStable(this Uuid uuid)
    {
        // Convert RFC4122 byte order to Guid using big-endian semantics.
        Span<byte> bytes = stackalloc byte[16];
        uuid.CopyTo(bytes);
        return new Guid(bytes, bigEndian: true);
    }
}
