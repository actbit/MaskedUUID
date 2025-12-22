namespace MaskedUUID.Sample.Models;

/// <summary>
/// Sample のデータモデル
/// </summary>
public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
