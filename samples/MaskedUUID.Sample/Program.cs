using MaskedUUID.AspNetCore.Extensions;
using MaskedUUID.AspNetCore.KeyProviders;
using MaskedUUID.AspNetCore.Services;
using MaskedUUID.AspNetCore.Types;
using MaskedUUID.Sample.KeyProviders;
using MaskedUUID.Sample.Services;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var mvcBuilder = builder.Services.AddControllers();

// Use AddOpenApi for MaskedGuid schema configuration
//builder.Services.AddOpenApi(x=>x.AddMaskedUUIDOpenApi());
Guid guid = new Guid("12345678-1234-1234-1234-1234567890ab");
// Use AddSwaggerGen for Swagger UI (to avoid XML comment processing issues with AddOpenApi)
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// MaskedUUID 関連サービスの登録
// Note: Tenant management is handled internally by KeyProvider implementation

// 1. IMaskedUUIDKeyProvider の実装を登録
//    (Tenant resolution is handled internally by the KeyProvider)
builder.Services.AddScoped<IMaskedUUIDKeyProvider, SampleUUIDv47KeyProvider>();

// 2. MaskedUUID ASP.NET Core 統合を登録
//    - Registers IMaskedUUIDService as Singleton
//    - Registers JSON コンバーター
//    - Registers ModelBinder プロバイダー
builder.Services.AddMaskedUUID();
mvcBuilder.AddMaskedUUIDModelBinder();
// Note: AddOpenApi() disabled due to XML comment processing NullReferenceException
// Use Swagger/Swashbuckle for API documentation instead
// builder.Services.AddOpenApi(x => x.AddMaskedUUIDOpenApi());

// Sample アプリケーション固有のサービス
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddLogging(config =>
{
    config.AddConsole();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Items API");
        options.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
