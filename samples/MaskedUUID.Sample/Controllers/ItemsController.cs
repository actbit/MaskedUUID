using MaskedUUID.AspNetCore.Types;
using MaskedUUID.Sample.Dtos;
using MaskedUUID.Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaskedUUID.Sample.Controllers;

/// <summary>
/// Item 管理 API
/// MaskedGuid 型を使用して、自動的に URL パラメータを変換
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    /// <summary>
    /// すべての Item を取得
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ItemDto>>> GetAll()
    {
        _logger.LogInformation("GetAll called");
        var items = await _itemService.GetAllItemsAsync();
        return Ok(items);
    }

    /// <summary>
    /// 特定の Item を ID で取得
    ///
    /// MaskedGuid 型により、URL の {itemId} が自動的に MaskedUUID から Guid に変換される
    /// 例: /api/items/abc123def... → Guid に変換
    /// </summary>
    [HttpGet("{itemId}")]
    public async Task<ActionResult<ItemDto>> GetById(MaskedGuid itemId)
    {
        _logger.LogInformation("GetById called with itemId: {ItemId}", itemId.Value);
        var item = await _itemService.GetItemAsync(itemId.Value);

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// 新しい Item を作成
    ///
    /// レスポンスの ItemDto の Id フィールドは自動的に Guid から MaskedUUID に変換される
    /// 例: Guid → "abc123def..." (MaskedUUID 文字列)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemRequest request)
    {
        _logger.LogInformation("Create called with name: {Name}", request.Name);

        if (string.IsNullOrEmpty(request.Name))
            return BadRequest("Name is required");

        var item = await _itemService.CreateItemAsync(request);
        return CreatedAtAction(nameof(GetById), new { itemId = item.Id }, item);
    }

    /// <summary>
    /// Item を更新
    ///
    /// MaskedGuid 型により URL の {itemId} が自動変換
    /// レスポンスの Id も自動的に MaskedUUID に変換
    /// </summary>
    [HttpPut("{itemId}")]
    public async Task<ActionResult<ItemDto>> Update(
        MaskedGuid itemId,
        [FromBody] UpdateItemRequest request)
    {
        _logger.LogInformation("Update called with itemId: {ItemId}", itemId.Value);

        var item = await _itemService.UpdateItemAsync(itemId.Value, request);

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    /// <summary>
    /// Item を削除
    ///
    /// MaskedGuid 型により URL の {itemId} が自動変換
    /// </summary>
    [HttpDelete("{itemId}")]
    public async Task<IActionResult> Delete(MaskedGuid itemId)
    {
        _logger.LogInformation("Delete called with itemId: {ItemId}", itemId.Value);

        var success = await _itemService.DeleteItemAsync(itemId.Value);

        if (!success)
            return NotFound();

        return NoContent();
    }
}
