using MaskedUUID.AspNetCore.Extensions;
using UUIDv47Sharp;
namespace MaskedUUID.AspNetCore.Tests;

public class Uuidv47RoundtripTests
{
    [Fact]
    public void GuidToUuid_Parse_Roundtrip_ProducesSameUuid()
    {
        var originalV7 = Guid.CreateVersion7();

        var newV7 = originalV7.ToUuid().ToGuid();


        Assert.Equal(newV7, originalV7);
    }

    [Fact]
    public void GuidToUuid_Workaround_Roundtrip_ReturnsOriginalGuid()
    {
        var originalV7 = Guid.CreateVersion7();

        var uuid = originalV7.ToUuidStable();
        var roundtripped = uuid.ToGuidStable();

        Assert.Equal(originalV7, roundtripped);
    }

    [Fact]
    public void GuidToUuid_Workaround_Roundtrip_PreservesNonTimestampBytes()
    {
        var originalV7 = Guid.CreateVersion7();

        var originalUuid = originalV7.ToUuidStable();
        var roundtrippedGuid = originalUuid.ToGuidStable();
        var roundtrippedUuid = roundtrippedGuid.ToUuidStable();

        Span<byte> originalBytes = stackalloc byte[16];
        Span<byte> roundtrippedBytes = stackalloc byte[16];
        originalUuid.CopyTo(originalBytes);
        roundtrippedUuid.CopyTo(roundtrippedBytes);

        // UUIDv7 timestamp is the first 48 bits (bytes 0-5).
        for (var i = 6; i < 16; i++)
        {
            Assert.Equal(originalBytes[i], roundtrippedBytes[i]);
        }
    }

    [Fact]
    public void GuidV7_UuidV7_UuidV4_GuidV4_TimestampAndVersionMasked_RandomBitsMatch()
    {
        var originalV7 = Guid.CreateVersion7();
        var key = new Key(0x0123456789ABCDEF, 0xFEDCBA9876543210);

        var uuidV7 = originalV7.ToUuidStable();
        var uuidV4 = Uuid47Codec.Encode(uuidV7, key);
        var guidV4 = uuidV4.ToGuidStable();

        Span<byte> v7Bytes = stackalloc byte[16];
        Span<byte> v4Bytes = stackalloc byte[16];
        Span<byte> v4GuidBytes = stackalloc byte[16];

        uuidV7.CopyTo(v7Bytes);
        uuidV4.CopyTo(v4Bytes);
        Uuid.Parse(guidV4.ToString()).CopyTo(v4GuidBytes);

        var v7TimestampDiff = CountTimestampByteDifferences(v7Bytes, v4Bytes);
        var v7GuidTimestampDiff = CountTimestampByteDifferences(v7Bytes, v4GuidBytes);

        MaskTimestampAndVersionNibble(v7Bytes);
        MaskTimestampAndVersionNibble(v4Bytes);
        MaskTimestampAndVersionNibble(v4GuidBytes);

        Assert.True(v7Bytes.SequenceEqual(v4Bytes));
        Assert.True(v7Bytes.SequenceEqual(v4GuidBytes));
        Assert.True(v7TimestampDiff > 0);
        Assert.True(v7GuidTimestampDiff > 0);

        static void MaskTimestampAndVersionNibble(Span<byte> bytes)
        {
            // Mask UUIDv7 timestamp (bytes 0-5) and version nibble (high 4 bits of byte 6).
            for (var i = 0; i < 6; i++)
            {
                bytes[i] = 0;
            }

            // Version bits are the high nibble of byte 6; clear them for comparison.
            bytes[6] &= 0x0F;
        }

        static int CountTimestampByteDifferences(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
        {
            var diff = 0;
            for (var i = 0; i < 6; i++)
            {
                if (left[i] != right[i])
                {
                    diff++;
                }
            }

            return diff;
        }
    }

    [Fact]
    public void GuidV7_UuidV7_UuidV4_GuidV4_IgnoreTimestampAndVersionVariantBytes_Match()
    {
        var originalV7 = Guid.CreateVersion7();
        var key = new Key(0x0123456789ABCDEF, 0xFEDCBA9876543210);

        var uuidV7 = originalV7.ToUuidStable();
        var uuidV4 = Uuid47Codec.Encode(uuidV7, key);
        var guidV4 = uuidV4.ToGuidStable();

        Span<byte> v7Bytes = stackalloc byte[16];
        Span<byte> v4Bytes = stackalloc byte[16];
        Span<byte> v4GuidBytes = stackalloc byte[16];

        uuidV7.CopyTo(v7Bytes);
        uuidV4.CopyTo(v4Bytes);
        Uuid.Parse(guidV4.ToString()).CopyTo(v4GuidBytes);

        MaskTimestampAndVersionVariantBytes(v7Bytes);
        MaskTimestampAndVersionVariantBytes(v4Bytes);
        MaskTimestampAndVersionVariantBytes(v4GuidBytes);

        Assert.True(v7Bytes.SequenceEqual(v4Bytes));
        Assert.True(v7Bytes.SequenceEqual(v4GuidBytes));

        static void MaskTimestampAndVersionVariantBytes(Span<byte> bytes)
        {
            // Mask timestamp bytes (0-5).
            for (var i = 0; i < 6; i++)
            {
                bytes[i] = 0;
            }

            // Mask version/variant surrounding bytes (6-9).
            for (var i = 6; i <= 9; i++)
            {
                bytes[i] = 0;
            }
        }
    }

    [Fact]
    public void GuidV7_UuidV7_UuidV4_GuidV4_CompareLowerTwoBlocks()
    {
        var originalV7 = Guid.CreateVersion7();
        var key = new Key(0x0123456789ABCDEF, 0xFEDCBA9876543210);

        var uuidV7 = originalV7.ToUuidStable();
        var uuidV4 = Uuid47Codec.Encode(uuidV7, key);
        var guidV4 = uuidV4.ToGuidStable();

        var v7Text = uuidV7.ToString();
        var v4Text = uuidV4.ToString();
        var v4GuidText = guidV4.ToString();

        var v7Lower = string.Join('-', v7Text.Split('-').Skip(3));
        var v4Lower = string.Join('-', v4Text.Split('-').Skip(3));
        var v4GuidLower = string.Join('-', v4GuidText.Split('-').Skip(3));

        Assert.Equal(v7Lower, v4Lower);
        Assert.Equal(v7Lower, v4GuidLower);
    }

    [Fact]
    public void UuidToGuid_FromToUuid_ReturnsOriginalGuid()
    {
        var originalV7 = Guid.CreateVersion7();

        var uuid = originalV7.ToUuid();
        var backToGuid = uuid.ToGuid();

        Assert.Equal(originalV7, backToGuid);
    }

    [Fact]
    public void Codec_EncodeDecode_Roundtrip_WithParsedUuid_ReturnsOriginalUuid()
    {
        var originalV7 = Guid.CreateVersion7();
        var uuid = Uuid.Parse(originalV7.ToString());
        var key = new Key(0x0123456789ABCDEF, 0xFEDCBA9876543210);

        var masked = Uuid47Codec.Encode(uuid, key);
        var decoded = Uuid47Codec.Decode(masked, key);

        Assert.Equal(uuid.ToString(), decoded.ToString());
    }

}
