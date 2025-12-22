using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using MaskedUUID.AspNetCore.Attributes;
using MaskedUUID.AspNetCore.ModelBinding;
using MaskedUUID.AspNetCore.Services;
using Moq;

namespace MaskedUUID.AspNetCore.Tests;

public class MaskedUUIDModelBinderProviderTests
{
    private readonly MaskedUUIDModelBinderProvider _provider;

    public MaskedUUIDModelBinderProviderTests()
    {
        _provider = new MaskedUUIDModelBinderProvider();
    }

    [Fact]
    public void GetBinder_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _provider.GetBinder(null!));
    }

    [Fact]
    public void GetBinder_WithNonGuidType_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(string));
        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.Setup(c => c.Metadata).Returns(metadata);

        // Act
        var result = _provider.GetBinder(contextMock.Object);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBinder_WithGuidTypeButNoValidatorMetadata_ReturnsNull()
    {
        // Arrange
        var metadata = CreateModelMetadata(typeof(Guid));
        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.Setup(c => c.Metadata).Returns(metadata);

        // Act
        var result = _provider.GetBinder(contextMock.Object);

        // Assert
        Assert.Null(result);
    }

    private static ModelMetadata CreateModelMetadata(Type modelType)
    {
        var provider = new EmptyModelMetadataProvider();
        return provider.GetMetadataForType(modelType);
    }
}
