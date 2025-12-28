using Microsoft.AspNetCore.Mvc.ModelBinding;
using MaskedUUID.AspNetCore.ModelBinding;
using MaskedUUID.AspNetCore.Types;
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
    public void GetBinder_WithPlainGuidType_ReturnsNull()
    {
        // Arrange - plain Guid type (not MaskedGuid) should NOT get binder anymore
        // Only MaskedGuid type parameters should be handled
        var metadata = CreateModelMetadata(typeof(Guid));
        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.Setup(c => c.Metadata).Returns(metadata);

        // Act
        var result = _provider.GetBinder(contextMock.Object);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBinder_WithPlainNullableGuidType_ReturnsNull()
    {
        // Arrange - plain nullable Guid type (not MaskedGuid?) should NOT get binder
        var metadata = CreateModelMetadata(typeof(Guid?));
        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.Setup(c => c.Metadata).Returns(metadata);

        // Act
        var result = _provider.GetBinder(contextMock.Object);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBinder_WithMaskedGuidType_ReturnsBinder()
    {
        // Arrange - MaskedGuid type should automatically get binder for URL parameter binding
        var metadata = CreateModelMetadata(typeof(MaskedGuid));

        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.Setup(c => c.Metadata).Returns(metadata);

        // Act
        var result = _provider.GetBinder(contextMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MaskedUUIDModelBinder>(result);
    }

    [Fact]
    public void GetBinder_WithNullableMaskedGuidType_ReturnsBinder()
    {
        // Arrange - nullable MaskedGuid type should also get binder
        var metadata = CreateModelMetadata(typeof(MaskedGuid?));

        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.Setup(c => c.Metadata).Returns(metadata);

        // Act
        var result = _provider.GetBinder(contextMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<MaskedUUIDModelBinder>(result);
    }

    private static ModelMetadata CreateModelMetadata(Type modelType)
    {
        var provider = new EmptyModelMetadataProvider();
        return provider.GetMetadataForType(modelType);
    }
}
