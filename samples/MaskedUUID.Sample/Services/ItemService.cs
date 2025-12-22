using MaskedUUID.AspNetCore.Extensions;
using MaskedUUID.Sample.Dtos;
using MaskedUUID.Sample.Models;

namespace MaskedUUID.Sample.Services;

/// <summary>
/// Item 管理サービス
/// In-memory ストレージ（デモンストレーション用）
/// </summary>
public interface IItemService
{
    Task<ItemDto?> GetItemAsync(Guid itemId);
    Task<List<ItemDto>> GetAllItemsAsync();
    Task<ItemDto> CreateItemAsync(CreateItemRequest request);
    Task<ItemDto?> UpdateItemAsync(Guid itemId, UpdateItemRequest request);
    Task<bool> DeleteItemAsync(Guid itemId);
}

public class ItemService : IItemService
{
    private readonly ILogger<ItemService> _logger;
    private static readonly Dictionary<Guid, Item> Items = new();

    public ItemService(ILogger<ItemService> logger)
    {
        _logger = logger;

        // Sample データ
        var sampleItem = new Item
        {
            Id = Guid.NewGuid(),
            Name = "Sample Item 1",
            Description = "This is a sample item",
            CreatedAt = DateTime.UtcNow
        };
        Items[sampleItem.Id] = sampleItem;
    }

    public async Task<ItemDto?> GetItemAsync(Guid itemId)
    {
        _logger.LogInformation("Getting item {ItemId}", itemId);

        if (Items.TryGetValue(itemId, out var item))
        {
            return await Task.FromResult(MapToDto(item));
        }

        _logger.LogWarning("Item not found: {ItemId}", itemId);
        return null;
    }

    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        _logger.LogInformation("Getting all items");
        return await Task.FromResult(Items.Values.Select(MapToDto).ToList());
    }

    public async Task<ItemDto> CreateItemAsync(CreateItemRequest request)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        Items[item.Id] = item;
        _logger.LogInformation("Item created: {ItemId}", item.Id);

        return await Task.FromResult(MapToDto(item));
    }

    public async Task<ItemDto?> UpdateItemAsync(Guid itemId, UpdateItemRequest request)
    {
        if (!Items.TryGetValue(itemId, out var item))
        {
            _logger.LogWarning("Item not found for update: {ItemId}", itemId);
            return null;
        }

        if (!string.IsNullOrEmpty(request.Name))
            item.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Description))
            item.Description = request.Description;

        _logger.LogInformation("Item updated: {ItemId}", itemId);
        return await Task.FromResult(MapToDto(item));
    }

    public async Task<bool> DeleteItemAsync(Guid itemId)
    {
        var removed = Items.Remove(itemId);

        if (removed)
        {
            _logger.LogInformation("Item deleted: {ItemId}", itemId);
        }
        else
        {
            _logger.LogWarning("Item not found for deletion: {ItemId}", itemId);
        }

        return await Task.FromResult(removed);
    }

    private static ItemDto MapToDto(Item item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        CreatedAt = item.CreatedAt
    };
}
