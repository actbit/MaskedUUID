using MaskedUUID.AspNetCore.KeyProviders;
using Microsoft.Extensions.Caching.Memory;

namespace MaskedUUID.Sample.KeyProviders;

/// <summary>
/// Sample 用の UUIDv47 キープロバイダー実装
/// 内部でテナント情報を解決し、キーを管理
/// メモリキャッシュを使用してキーを保持
/// 実際の環境では、Database や Redis から取得します
/// </summary>
public class SampleUUIDv47KeyProvider : IMaskedUUIDKeyProvider
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<SampleUUIDv47KeyProvider> _logger;
    private const string CacheKeyPrefix = "UUIDv47_Keys_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(7);

    // Sample用の固定キー（本来は DB/Redis から取得）
    private static readonly Dictionary<Guid, (ulong K0, ulong K1)> SampleKeys = new()
    {
        {
            Guid.Parse("12345678-1234-5678-1234-567812345678"),
            (0x0123456789ABCDEF, 0xFEDCBA9876543210)
        }
    };

    public SampleUUIDv47KeyProvider(IMemoryCache cache, ILogger<SampleUUIDv47KeyProvider> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 現在のコンテキストのキーを取得
    /// Sample では固定のテナント ID を使用
    /// 実装が内部でテナント ID を解決
    /// </summary>
    public async Task<(ulong K0, ulong K1)> GetKeysAsync()
    {
        // Sample では固定のテナント ID
        var tenantId = Guid.Parse("12345678-1234-5678-1234-567812345678");

        var cacheKey = $"{CacheKeyPrefix}{tenantId}";

        if (_cache.TryGetValue(cacheKey, out (ulong k0, ulong k1) cachedKeys))
        {
            _logger.LogDebug("Keys found in cache");
            return cachedKeys;
        }

        // Sample: 辞書から取得（本来は DB/Redis）
        if (!SampleKeys.TryGetValue(tenantId, out var keys))
        {
            _logger.LogWarning("No keys found for tenant {TenantId}", tenantId);
            throw new InvalidOperationException($"No keys configured for tenant {tenantId}");
        }

        // キャッシュに追加
        _cache.Set(cacheKey, keys, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        });

        _logger.LogDebug("Keys fetched and cached");
        return await Task.FromResult(keys);
    }

    /// <summary>
    /// 同期版：キーを取得
    /// </summary>
    public (ulong K0, ulong K1) GetKeysSynchronous()
    {
        var tenantId = Guid.Parse("12345678-1234-5678-1234-567812345678");

        var cacheKey = $"{CacheKeyPrefix}{tenantId}";

        if (_cache.TryGetValue(cacheKey, out (ulong k0, ulong k1) cachedKeys))
        {
            _logger.LogDebug("Keys found in cache (sync)");
            return cachedKeys;
        }

        if (!SampleKeys.TryGetValue(tenantId, out var keys))
        {
            _logger.LogWarning("No keys found for tenant {TenantId}", tenantId);
            throw new InvalidOperationException($"No keys configured for tenant {tenantId}");
        }

        _cache.Set(cacheKey, keys, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        });

        return keys;
    }
}
