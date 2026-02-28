# MaskedUUID.AspNetCore

ASP.NET Core で GUID を「MaskedUUID」形式へ変換するためのライブラリです。

## 仕様（重要）

### MaskedGuid のマスク範囲
- `MaskedGuid` は **JSON の入出力のみ** でマスク（エンコード/デコード）されます。
- `MaskedGuid.ToString()` は **生の `Guid` 文字列** を返します（マスク文字列ではありません）。

### JSON シリアライズ時の動作
- `MaskedGuid` は JSON 変換時に `IMaskedUUIDService` を使ってマスク化します。
- `Guid.Empty`（空 GUID） は JSON 出力で `null` になります。
- JSON 入力の `null` / `""`（空文字）は `Guid.Empty` として扱われます。

### 動作スコープ（HTTP リクエスト内）
- `MaskedGuid` 用の JSON 変換は `HttpContext` から `IMaskedUUIDService` を解決します。
- そのため **HTTP リクエスト外**（バックグラウンド処理や非 Web 環境）で
  `MaskedGuid` を JSON シリアライズ/デシリアライズすると例外になります。

### ModelBinder の対象
- URL/クエリのバインドは **`MaskedGuid` / `MaskedGuid?` 型のみ** 対象です。
- `[MaskedUUID]` 属性は使用しません（属性ベースの処理はありません）。
- `MaskedGuid?` はデコード結果が `Guid.Empty` の場合 **`null`** として扱われます。
- `MaskedGuid` はデコード結果が `Guid.Empty` の場合 **`Guid.Empty`** のまま扱われます。
  - JSON 変換でも `Guid.Empty` は `null` として出力され、読み込み時は `Guid.Empty` として扱われます。

## 使い方（概要）

1. `IMaskedUUIDKeyProvider` を登録
2. `IMaskedUUIDService` を登録
3. `AddMaskedUUID()` と `AddMaskedUUIDModelBinder()` を追加

サンプルは `samples/MaskedUUID.Sample` を参照してください。

## MVC / API Controllers

```csharp
// KeyProviderを登録（Singleton推奨、Scopedも可）
builder.Services.AddSingleton<IMaskedUUIDKeyProvider, MyKeyProvider>();

// MaskedUUID対応を追加
builder.Services.AddMaskedUUID();

// ModelBinderを追加（URL/クエリパラメータ用）
mvcBuilder.AddMaskedUUIDModelBinder();

// または一括で
mvcBuilder.AddMaskedUUIDControllers();
```

## SignalR

SignalR対応には `AddMaskedUUID()` 拡張メソッドを使用します。

```csharp
// KeyProviderを登録
builder.Services.AddSingleton<IMaskedUUIDKeyProvider, MyKeyProvider>();

// SignalRにMaskedUUID対応を追加
builder.Services.AddSignalR().AddMaskedUUID();
```

### SignalRスコープ対応

SignalR用のコンバーターは以下の優先順位でサービスを解決します：

1. **Hubスコープ** - Hub呼び出し内のスコープ（HubFilter経由）
2. **HttpContextスコープ** - HTTP接続の場合
3. **一時スコープ** - フォールバック

これにより、KeyProvider/Serviceが **Singleton / Scoped / Transient** のいずれでも正しく動作します。

### 開発用（参照キー使用）

```csharp
// 開発/テスト用に参照キーを使用（本番では使用しないでください）
builder.Services.AddSignalR().AddMaskedUUIDWithReferenceKeys();
```

## OpenAPI / Swagger

.NET 9 / .NET 10 両方に対応しています。

```csharp
// OpenAPIスキーマトランスフォーマーを追加
builder.Services.AddOpenApi(options => options.AddMaskedGuidSchemaTransformer());
```

.NET 10では `JsonSchemaType` enum、.NET 9では文字列ベースの型指定に自動的に対応します。
