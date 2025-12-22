using MaskedUUID.AspNetCore.Attributes;

namespace MaskedUUID.Sample.Dtos;

/// <summary>
/// Item のレスポンス DTO
/// [MaskedUUID] 属性がついた Guid フィールドは自動的に MaskedUUID に変換される
/// </summary>
public class ItemDto
{
    /// <summary>
    /// Item ID（MaskedUUID として自動変換）
    /// レスポンスでは Guid が MaskedUUID 文字列に変換され、
    /// リクエストでは MaskedUUID 文字列が Guid に変換される
    /// </summary>
    [MaskedUUID]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Item 作成リクエスト DTO
/// </summary>
public class CreateItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Item 更新リクエスト DTO
/// </summary>
public class UpdateItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
