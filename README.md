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
