using MaskedUUID.AspNetCore.Types;

namespace MaskedUUID.Sample.Dtos;

/// <summary>
/// Item のレスポンス DTO
/// MaskedGuid 型のフィールドは自動的に MaskedUUID に変換される
/// </summary>
public class ItemDto
{
    /// <summary>
    /// Item ID（MaskedGuid として自動変換）
    /// レスポンスでは MaskedGuid が MaskedUUID 文字列に変換され、
    /// リクエストでは MaskedUUID 文字列が MaskedGuid に変換される
    /// </summary>
    public MaskedGuid Id { get; set; }

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
