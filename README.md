# MaskedUUID.AspNetCore

ASP.NET Core で UUIDv7 のタイムスタンプ部を [UUIDv47](https://github.com/actbit/uuidv47) アルゴリズムでマスクし、可逆的に変換するライブラリです。

## 概要

このライブラリは [UUIDv47Sharp](https://github.com/actbit/UUIDv47Sharp) （C#実装）を使用して、GUID を可逆的にマスク化します。

### UUIDv47 とは

UUIDv47 は UUIDv7 のタイムスタンプ部を隠蔽するための可逆エンコーディング形式です。

- **UUIDv7 → UUIDv47**: 128bit キーで XOR 暗号化し、タイムスタンプを隠蔽
- **可逆性**: 同じキーを使用すれば、UUIDv47 から元の UUIDv7 を復元可能
- **キー管理**: `IMaskedUUIDKeyProvider` でキーを提供（テナントごとに異なるキーなど柔軟に対応可能）

### 実装

- [uuidv47](https://github.com/actbit/uuidv47) - Go実装（オリジナル）
- [UUIDv47Sharp](https://github.com/actbit/UUIDv47Sharp) - C#実装

### なぜ UUIDv47 を使うのか

UUIDv7 は先頭48ビットにタイムスタンプを含むため、そのまま公開すると **リソースの作成日時が暴露** されてしまいます。

これにより以下のような情報漏洩リスクがあります：

- ユーザー登録日時、注文日時などの **業務情報が推測可能**
- 新規ユーザー数や **成長率が外部に漏洩**
- 競合他社に **ビジネス状況を把握される**

このライブラリは、以下のタイミングで自動的に UUIDv7 ↔ UUIDv47 変換を行います：

- **API レスポンス**: `MaskedGuid` 型のプロパティが JSON に出力される際、UUIDv47 に変換
- **API リクエスト**: UUIDv47 文字列を受信時、自動的に元の GUID に復元
- **URL パラメータ**: `/items/{id}` のような URL で UUIDv47 の ID を受け取り、自動復元
- **SignalR**: Hub の送受信で `MaskedGuid` 型のプロパティを自動変換

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

.NET 10以降 / .NET 9 に対応しています。

```csharp
// OpenAPIスキーマトランスフォーマーを追加
builder.Services.AddOpenApi(options => options.AddMaskedGuidSchemaTransformer());
```

.NET 10以降では `JsonSchemaType` enum、.NET 9では文字列ベースの型指定に自動的に対応します。
