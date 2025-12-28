using MaskedUUID.AspNetCore.Extensions;
using MaskedUUID.AspNetCore.KeyProviders;
using MaskedUUID.AspNetCore.Services;
using MaskedUUID.Sample.KeyProviders;
using MaskedUUID.Sample.OpenApi;
using MaskedUUID.Sample.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<MaskedGuidSchemaTransformer>();
});
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
//builder.Services.AddSingleton<IApiDescriptionProvider, NullModelMetadataFixApiDescriptionProvider>();

// MaskedUUID 関連サービスの登録
// Note: Tenant management is handled internally by KeyProvider implementation

// 1. IMaskedUUIDKeyProvider の実装を登録
//    (Tenant resolution is handled internally by the KeyProvider)
builder.Services.AddScoped<IMaskedUUIDKeyProvider, SampleUUIDv47KeyProvider>();

// 2. IMaskedUUIDService を登録（キープロバイダーを使用）
builder.Services.AddScoped<IMaskedUUIDService, MaskedUUIDService>();

// 3. MaskedUUID ASP.NET Core 統合を登録
//    - JSON コンバーターの登録
//    - ModelBinder プロバイダーの登録
builder.Services.AddMaskedUUID();
builder.Services.AddControllers().AddMaskedUUIDModelBinder();

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
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Items API"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
