# MaskedUUID.AspNetCore

A library for ASP.NET Core that masks the timestamp portion of UUIDv7 using the [UUIDv47](https://github.com/actbit/uuidv47) algorithm with reversible transformation.

## Overview

This library uses [UUIDv47Sharp](https://github.com/actbit/UUIDv47Sharp) (C# implementation) to reversibly mask GUIDs.

### What is UUIDv47?

UUIDv47 is a reversible encoding format that hides the timestamp portion of UUIDv7.

- **UUIDv7 → UUIDv47**: XOR encryption with a 128-bit key to hide the timestamp
- **Reversibility**: The original UUIDv7 can be restored from UUIDv47 using the same key
- **Key Management**: Provide keys via `IMaskedUUIDKeyProvider` (flexible support for different keys per tenant, etc.)

### Implementations

- [uuidv47](https://github.com/actbit/uuidv47) - Go implementation (original)
- [UUIDv47Sharp](https://github.com/actbit/UUIDv47Sharp) - C# implementation

### Why use UUIDv47?

UUIDv7 contains a 48-bit timestamp at the beginning, which means **exposing it directly reveals the resource creation timestamp**.

This poses the following information leakage risks:

- **Business information** such as user registration time and order time becomes guessable
- **Growth rate** and new user count leak to the outside
- **Business status** becomes visible to competitors

This library automatically performs UUIDv7 ↔ UUIDv47 conversion at the following timing:

- **API Response**: `MaskedGuid` type properties are converted to UUIDv47 when output to JSON
- **API Request**: UUIDv47 strings are automatically restored to the original GUID when received
- **URL Parameter**: Accept UUIDv47 IDs in URLs like `/items/{id}` with automatic restoration
- **SignalR**: Automatic conversion of `MaskedGuid` type properties in Hub send/receive

## Specifications (Important)

### MaskedGuid Masking Scope
- `MaskedGuid` is masked (encoded/decoded) **only during JSON input/output**.
- `MaskedGuid.ToString()` returns the **raw `Guid` string** (not the masked string).

### JSON Serialization Behavior
- `MaskedGuid` uses `IMaskedUUIDService` for masking during JSON conversion.
- `Guid.Empty` (empty GUID) becomes `null` in JSON output.
- `null` / `""` (empty string) in JSON input is treated as `Guid.Empty`.

### Operation Scope (Within HTTP Request)
- JSON conversion for `MaskedGuid` resolves `IMaskedUUIDService` from `HttpContext`.
- Therefore, JSON serialization/deserialization of `MaskedGuid` **outside HTTP requests** (background processing, non-Web environments) will throw an exception.

### ModelBinder Target
- URL/Query binding targets **only `MaskedGuid` / `MaskedGuid?` types**.
- `[MaskedUUID]` attribute is not used (no attribute-based processing).
- `MaskedGuid?` is treated as **`null`** when the decode result is `Guid.Empty`.
- `MaskedGuid` remains as **`Guid.Empty`** when the decode result is `Guid.Empty`.
  - In JSON conversion, `Guid.Empty` is output as `null` and treated as `Guid.Empty` when read.

## Usage (Overview)

1. Register `IMaskedUUIDKeyProvider`
2. Register `IMaskedUUIDService`
3. Add `AddMaskedUUID()` and `AddMaskedUUIDModelBinder()`

See `samples/MaskedUUID.Sample` for samples.

## MVC / API Controllers

```csharp
// Register KeyProvider (Singleton recommended, Scoped also acceptable)
builder.Services.AddSingleton<IMaskedUUIDKeyProvider, MyKeyProvider>();

// Add MaskedUUID support
builder.Services.AddMaskedUUID();

// Add ModelBinder (for URL/query parameters)
mvcBuilder.AddMaskedUUIDModelBinder();

// Or all at once
mvcBuilder.AddMaskedUUIDControllers();
```

## SignalR

Use the `AddMaskedUUID()` extension method for SignalR support.

```csharp
// Register KeyProvider
builder.Services.AddSingleton<IMaskedUUIDKeyProvider, MyKeyProvider>();

// Add MaskedUUID support to SignalR
builder.Services.AddSignalR().AddMaskedUUID();
```

### SignalR Scope Support

SignalR converters resolve services in the following priority order:

1. **Hub Scope** - Scope within Hub invocation (via HubFilter)
2. **HttpContext Scope** - For HTTP connections
3. **Transient Scope** - Fallback

This ensures correct operation whether KeyProvider/Service is **Singleton / Scoped / Transient**.

### Development (Reference Key Usage)

```csharp
// Use reference keys for development/testing (do not use in production)
builder.Services.AddSignalR().AddMaskedUUIDWithReferenceKeys();
```

## OpenAPI / Swagger

Supports .NET 10+ and .NET 9.

```csharp
// Add OpenAPI schema transformer
builder.Services.AddOpenApi(options => options.AddMaskedGuidSchemaTransformer());
```

Automatically handles `JsonSchemaType` enum in .NET 10+ and string-based type specification in .NET 9.
