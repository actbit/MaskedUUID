using Microsoft.Extensions.Logging;
using MaskedUUID.AspNetCore.KeyProviders;
using MaskedUUID.AspNetCore.Services;
using Moq;

namespace MaskedUUID.AspNetCore.Tests;

public class MaskedUUIDServiceTests
{
    private readonly Mock<IMaskedUUIDKeyProvider> _keyProviderMock;
    private readonly Mock<ILogger<MaskedUUIDService>> _loggerMock;
    private readonly IMaskedUUIDService _service;
    private readonly Guid _testGuid = Guid.Parse("12345678-1234-5678-1234-567812345678");
    private const ulong TestK0 = 0x0123456789ABCDEF;
    private const ulong TestK1 = 0xFEDCBA9876543210;

    public MaskedUUIDServiceTests()
    {
        _keyProviderMock = new Mock<IMaskedUUIDKeyProvider>();
        _loggerMock = new Mock<ILogger<MaskedUUIDService>>();

        // Setup default mock behavior
        _keyProviderMock.Setup(k => k.GetKeysAsync())
            .ReturnsAsync((TestK0, TestK1));
        _keyProviderMock.Setup(k => k.GetKeysSynchronous())
            .Returns((TestK0, TestK1));

        _service = new MaskedUUIDService(_keyProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task EncodeAsync_WithValidGuid_ReturnsEncodedString()
    {
        // Act
        var result = await _service.EncodeAsync(_testGuid);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.IsType<string>(result);
        _keyProviderMock.Verify(k => k.GetKeysAsync(), Times.Once);
    }

    [Fact]
    public async Task EncodeAsync_CallsKeyProvider()
    {
        // Act
        await _service.EncodeAsync(_testGuid);

        // Assert - Verify that KeyProvider was called
        _keyProviderMock.Verify(k => k.GetKeysAsync(), Times.Once);
    }

    [Fact]
    public async Task EncodeListAsync_WithValidGuids_ReturnsEncodedList()
    {
        // Arrange
        var guids = new List<Guid> { _testGuid, Guid.NewGuid() };

        // Act
        var result = await _service.EncodeListAsync(guids);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(guids.Count, result.Count);
        Assert.All(result, item => Assert.NotEmpty(item));
    }

    [Fact]
    public async Task EncodeListAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var guids = new List<Guid>();

        // Act
        var result = await _service.EncodeListAsync(guids);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void EncodeSynchronous_WithValidGuid_ReturnsEncodedString()
    {
        // Act
        var result = _service.EncodeSynchronous(_testGuid);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        _keyProviderMock.Verify(k => k.GetKeysSynchronous(), Times.Once);
    }

    [Fact]
    public async Task DecodeAsync_WithValidMaskedUuid_ReturnsGuidType()
    {
        // Arrange - First encode a GUID
        var encoded = await _service.EncodeAsync(_testGuid);
        // Reset KeyProvider Mock to ensure consistent keys
        _keyProviderMock.Setup(k => k.GetKeysAsync())
            .ReturnsAsync((TestK0, TestK1));

        // Act
        var decoded = await _service.DecodeAsync(encoded);

        // Assert
        Assert.IsType<Guid>(decoded);
    }

    [Fact]
    public async Task DecodeListAsync_WithValidMaskedUuids_ReturnsGuidList()
    {
        // Arrange - Encode some GUIDs first
        var originalGuids = new List<Guid> { _testGuid, Guid.NewGuid() };
        var encodedList = await _service.EncodeListAsync(originalGuids);

        // Reset keys for decoding
        _keyProviderMock.Setup(k => k.GetKeysAsync())
            .ReturnsAsync((TestK0, TestK1));

        // Act
        var decodedList = await _service.DecodeListAsync(encodedList);

        // Assert
        Assert.NotNull(decodedList);
        Assert.Equal(originalGuids.Count, decodedList.Count);
    }

    [Fact]
    public void DecodeSynchronous_WithValidMaskedUuid_ReturnsGuidType()
    {
        // Arrange
        var encoded = _service.EncodeSynchronous(_testGuid);

        // Reset keys for decoding
        _keyProviderMock.Setup(k => k.GetKeysSynchronous())
            .Returns((TestK0, TestK1));

        // Act
        var decoded = _service.DecodeSynchronous(encoded);

        // Assert
        Assert.IsType<Guid>(decoded);
    }
}
