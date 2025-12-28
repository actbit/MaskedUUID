using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MaskedUUID.AspNetCore.KeyProviders;

/// <summary>
/// Reference implementation that reads UUIDv47 mask keys from configuration.
/// </summary>
public sealed class ReferenceUUIDv47KeyProvider : IMaskedUUIDKeyProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReferenceUUIDv47KeyProvider> _logger;

    public ReferenceUUIDv47KeyProvider(
        IConfiguration configuration,
        ILogger<ReferenceUUIDv47KeyProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<(ulong K0, ulong K1)> GetKeysAsync()
    {
        return Task.FromResult(GetKeysSynchronous());
    }

    public (ulong K0, ulong K1) GetKeysSynchronous()
    {
        // Expected configuration:
        // MaskedUUID:Keys:K0 = "0x0123456789ABCDEF" (hex) or "81985529216486895" (decimal)
        // MaskedUUID:Keys:K1 = "0xFEDCBA9876543210" (hex) or "18364758544493064720" (decimal)
        var k0Raw = _configuration["MaskedUUID:Keys:K0"];
        var k1Raw = _configuration["MaskedUUID:Keys:K1"];

        if (string.IsNullOrWhiteSpace(k0Raw) || string.IsNullOrWhiteSpace(k1Raw))
        {
            throw new InvalidOperationException(
                "MaskedUUID keys are missing. Set MaskedUUID:Keys:K0 and MaskedUUID:Keys:K1 in configuration.");
        }

        var k0 = ParseUlong(k0Raw);
        var k1 = ParseUlong(k1Raw);

        _logger.LogDebug("MaskedUUID keys loaded from configuration.");
        return (k0, k1);
    }

    private static ulong ParseUlong(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[2..];
            return ulong.Parse(trimmed, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }

        if (ulong.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return ulong.Parse(trimmed, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }
}
