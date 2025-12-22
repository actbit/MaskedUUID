using Microsoft.AspNetCore.Mvc.ModelBinding;
using MaskedUUID.AspNetCore.ModelBinding;
using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.Types;
using Moq;

namespace MaskedUUID.AspNetCore.Tests;

public class MaskedUUIDModelBinderTests
{
    private readonly Mock<IMaskedUUIDService> _serviceMock;
    private readonly MaskedUUIDModelBinder _binder;
    private readonly Guid _testGuid = Guid.Parse("12345678-1234-5678-1234-567812345678");
    private readonly MaskedGuid _testMaskedGuid;

    public MaskedUUIDModelBinderTests()
    {
        _serviceMock = new Mock<IMaskedUUIDService>();
        _binder = new MaskedUUIDModelBinder(_serviceMock.Object);
        _testMaskedGuid = new MaskedGuid(_testGuid);
    }

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MaskedUUIDModelBinder(null!));
    }

    [Fact]
    public async Task BindModelAsync_WithNullBindingContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _binder.BindModelAsync(null!));
    }

    [Fact]
    public async Task BindModelAsync_WithNoValue_DoesNotSetResult()
    {
        // Arrange - when no value provided for MaskedGuid parameter
        var valueProviderResult = ValueProviderResult.None;
        var modelName = "itemId";
        var resultValue = ModelBindingResult.Failed();
        var bindingContext = CreateMockBindingContext(modelName, valueProviderResult, resultValue, typeof(MaskedGuid));

        // Act
        await _binder.BindModelAsync(bindingContext);

        // Assert
        // When ValueProviderResult.None, the Result should not be set
        Assert.Equal(resultValue, bindingContext.Result);
    }

    [Fact]
    public async Task BindModelAsync_WithValidMaskedUuid_BindsDecodedGuidToMaskedGuid()
    {
        // Arrange - MaskedGuid type parameter from URL
        var maskedUuid = "MASKED_UUID_VALUE";
        _serviceMock.Setup(s => s.DecodeSynchronous(maskedUuid))
            .Returns(_testGuid);

        var valueProviderResult = new ValueProviderResult(new string[] { maskedUuid });
        var modelName = "itemId";
        var bindingContext = CreateMockBindingContext(modelName, valueProviderResult, ModelBindingResult.Failed(), typeof(MaskedGuid));

        // Act
        await _binder.BindModelAsync(bindingContext);

        // Assert
        Assert.True(bindingContext.Result.IsModelSet);
        var resultModel = bindingContext.Result.Model;
        Assert.IsType<MaskedGuid>(resultModel);
        Assert.Equal(_testMaskedGuid, (MaskedGuid)resultModel!);
        _serviceMock.Verify(s => s.DecodeSynchronous(maskedUuid), Times.Once);
    }

    [Fact]
    public async Task BindModelAsync_WithValidMaskedUuid_BindsDecodedGuidToNullableMaskedGuid()
    {
        // Arrange - nullable MaskedGuid? type parameter
        var maskedUuid = "MASKED_UUID_VALUE";
        _serviceMock.Setup(s => s.DecodeSynchronous(maskedUuid))
            .Returns(_testGuid);

        var valueProviderResult = new ValueProviderResult(new string[] { maskedUuid });
        var modelName = "filterId";
        var bindingContext = CreateMockBindingContext(modelName, valueProviderResult, ModelBindingResult.Failed(), typeof(MaskedGuid?));

        // Act
        await _binder.BindModelAsync(bindingContext);

        // Assert
        Assert.True(bindingContext.Result.IsModelSet);
        var resultModel = bindingContext.Result.Model;
        Assert.IsType<MaskedGuid>(resultModel);
        Assert.Equal(_testMaskedGuid, (MaskedGuid)resultModel!);
        _serviceMock.Verify(s => s.DecodeSynchronous(maskedUuid), Times.Once);
    }

    private static ModelBindingContext CreateMockBindingContext(
        string modelName,
        ValueProviderResult valueProviderResult,
        ModelBindingResult initialResult,
        Type? modelType = null)
    {
        var contextMock = new Mock<ModelBindingContext>();
        contextMock.Setup(c => c.ModelName).Returns(modelName);
        contextMock.Setup(c => c.ValueProvider.GetValue(modelName)).Returns(valueProviderResult);
        contextMock.Setup(c => c.ModelType).Returns(modelType ?? typeof(MaskedGuid));
        contextMock.Setup(c => c.ModelState).Returns(new ModelStateDictionary());
        contextMock.Setup(c => c.Result).Returns(initialResult);

        // Allow setting Result
        var result = initialResult;
        contextMock.SetupSet(c => c.Result = It.IsAny<ModelBindingResult>())
            .Callback<ModelBindingResult>(r => result = r);
        contextMock.SetupGet(c => c.Result).Returns(() => result);

        return contextMock.Object;
    }
}
