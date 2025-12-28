using System.Buffers;
using System.Text;
using System.Text.Json;
using MaskedUUID.AspNetCore.Json;
using MaskedUUID.AspNetCore.Services;
using Moq;

namespace MaskedUUID.AspNetCore.Tests;

public class MaskedUUIDGuidConverterTests
{
    private readonly Mock<IMaskedUUIDService> _serviceMock;
    private readonly MaskedUUIDGuidConverter _converter;
    private readonly Guid _testGuid = Guid.Parse("12345678-1234-5678-1234-567812345678");

    public MaskedUUIDGuidConverterTests()
    {
        _serviceMock = new Mock<IMaskedUUIDService>();
        _converter = new MaskedUUIDGuidConverter(_serviceMock.Object);
    }

    [Fact]
    public void Read_WithValidMaskedUuid_ReturnsDecodedGuid()
    {
        // Arrange
        var maskedUuid = "MASKED_UUID_VALUE";
        _serviceMock.Setup(s => s.DecodeSynchronous(maskedUuid))
            .Returns(_testGuid);

        var json = Encoding.UTF8.GetBytes($"\"{maskedUuid}\"");
        var reader = new Utf8JsonReader(json);
        reader.Read(); // Move to the value token

        // Act
        var result = _converter.Read(ref reader, typeof(Guid), new JsonSerializerOptions());

        // Assert
        Assert.Equal(_testGuid, result);
        _serviceMock.Verify(s => s.DecodeSynchronous(maskedUuid), Times.Once);
    }

    [Fact]
    public void Read_WithNullValue_ReturnsEmpty()
    {
        // Arrange
        var jsonBytes = Encoding.UTF8.GetBytes("null");
        var reader = new Utf8JsonReader(jsonBytes);
        reader.Read(); // Move to null token

        // Act
        var result = _converter.Read(ref reader, typeof(Guid), new JsonSerializerOptions());

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void Read_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var jsonBytes = Encoding.UTF8.GetBytes("\"\"");
        var reader = new Utf8JsonReader(jsonBytes);
        reader.Read(); // Move to string token

        // Act
        var result = _converter.Read(ref reader, typeof(Guid), new JsonSerializerOptions());

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void Read_WithInvalidMaskedUuid_ThrowsJsonException()
    {
        // Arrange
        var invalidUuid = "INVALID";
        _serviceMock.Setup(s => s.DecodeSynchronous(invalidUuid))
            .Throws(new InvalidOperationException("Invalid MaskedUUID"));

        var jsonBytes = Encoding.UTF8.GetBytes($"\"{invalidUuid}\"");
        var reader = new Utf8JsonReader(jsonBytes);
        reader.Read(); // Move to string token

        // Act & Assert
        // Note: Can't use Assert.Throws with ref parameter, so we test the exception separately
        try
        {
            _converter.Read(ref reader, typeof(Guid), new JsonSerializerOptions());
            Assert.Fail("Expected JsonException");
        }
        catch (JsonException)
        {
            // Expected
        }
    }

    [Fact]
    public void Write_WithValidGuid_WritesEncodedString()
    {
        // Arrange
        var encodedValue = "ENCODED_MASKED_UUID";
        _serviceMock.Setup(s => s.EncodeSynchronous(_testGuid))
            .Returns(encodedValue);

        var options = new JsonSerializerOptions();
        using var writer = new Utf8JsonWriter(new ArrayBufferWriter<byte>());

        // Act
        _converter.Write(writer, _testGuid, options);

        // Assert
        _serviceMock.Verify(s => s.EncodeSynchronous(_testGuid), Times.Once);
    }

    [Fact]
    public void Write_WithEmptyGuid_WritesNull()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        using var writer = new Utf8JsonWriter(new ArrayBufferWriter<byte>());

        // Act
        _converter.Write(writer, Guid.Empty, options);

        // Assert
        _serviceMock.Verify(s => s.EncodeSynchronous(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public void Write_WithEncodingError_ThrowsJsonException()
    {
        // Arrange
        _serviceMock.Setup(s => s.EncodeSynchronous(_testGuid))
            .Throws(new InvalidOperationException("Encoding failed"));

        var options = new JsonSerializerOptions();
        using var writer = new Utf8JsonWriter(new ArrayBufferWriter<byte>());

        // Act & Assert
        Assert.Throws<JsonException>(() => _converter.Write(writer, _testGuid, options));
    }
}
