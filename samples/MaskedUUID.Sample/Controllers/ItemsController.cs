using MaskedUUID.AspNetCore.Types;
using MaskedUUID.Sample.Dtos;
using MaskedUUID.Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace MaskedUUID.Sample.Controllers;

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


    [HttpGet]
    public async Task<ActionResult<List<ItemDto>>> GetAll()
    {
        _logger.LogInformation("GetAll called");
        var items = await _itemService.GetAllItemsAsync();
        return Ok(items);
    }

    [HttpGet("{itemId}")]
    public async Task<ActionResult<ItemDto>> GetById([FromRoute] MaskedGuid itemId)
    {
        _logger.LogInformation("GetById called with itemId: {ItemId}", itemId.Value);
        var item = await _itemService.GetItemAsync(itemId.Value);

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemRequest request)
    {
        _logger.LogInformation("Create called with name: {Name}", request.Name);

        if (string.IsNullOrEmpty(request.Name))
            return BadRequest("Name is required");

        var item = await _itemService.CreateItemAsync(request);
        return CreatedAtAction(nameof(GetById), new { itemId = item.Id }, item);
    }

    [HttpPut("{itemId}")]
    public async Task<ActionResult<ItemDto>> Update(
        [FromRoute] MaskedGuid itemId,
        [FromBody] UpdateItemRequest request)
    {
        _logger.LogInformation("Update called with itemId: {ItemId}", itemId.Value);

        var item = await _itemService.UpdateItemAsync(itemId.Value, request);

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpDelete("{itemId}")]
    public async Task<IActionResult> Delete([FromRoute] MaskedGuid itemId)
    {
        _logger.LogInformation("Delete called with itemId: {ItemId}", itemId.Value);

        var success = await _itemService.DeleteItemAsync(itemId.Value);

        if (!success)
            return NotFound();

        return NoContent();
    }
}
