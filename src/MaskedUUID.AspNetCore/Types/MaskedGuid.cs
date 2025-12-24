namespace MaskedUUID.AspNetCore.Types;

/// <summary>
/// マスク済み GUID を表す型
/// JsonConverter で自動的に暗号化/復号化される
/// [MaskedUUID] 属性を持つプロパティに使用
/// </summary>
public readonly struct MaskedGuid :
    IEquatable<MaskedGuid>,
    IComparable<MaskedGuid>,
    IComparable,
    IEquatable<Guid>,
    IComparable<Guid>,
    IFormattable,
    ISpanParsable<MaskedGuid>
{
    public Guid Value { get; }

    public MaskedGuid(Guid value)
    {
        Value = value;
    }

    // 暗黙変換：Guid から MaskedGuid へ
    public static implicit operator MaskedGuid(Guid guid) => new(guid);

    // 暗黙変換：MaskedGuid から Guid へ
    public static implicit operator Guid(MaskedGuid maskedGuid) => maskedGuid.Value;

    // 等価性判定
    public override bool Equals(object? obj) => obj is MaskedGuid guid && Equals(guid);

    public bool Equals(MaskedGuid other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public int CompareTo(MaskedGuid other) => Value.CompareTo(other.Value);

    // IComparable implementation
    public int CompareTo(object? obj)
    {
        if (obj is MaskedGuid maskedGuid)
            return CompareTo(maskedGuid);
        throw new ArgumentException($"Object must be of type {nameof(MaskedGuid)}", nameof(obj));
    }

    // IEquatable<Guid> implementation
    public bool Equals(Guid other) => Value.Equals(other);

    // IComparable<Guid> implementation
    public int CompareTo(Guid other) => Value.CompareTo(other);

    // IFormattable implementation
    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);

    // ISpanParsable<MaskedGuid> implementation
    public static MaskedGuid Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => new(Guid.Parse(s, provider));

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out MaskedGuid result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new(guid);
            return true;
        }
        result = default;
        return false;
    }

    public static MaskedGuid Parse(string s, IFormatProvider? provider)
        => new(Guid.Parse(s, provider));

    public static bool TryParse(string? s, IFormatProvider? provider, out MaskedGuid result)
    {
        if (Guid.TryParse(s, provider, out var guid))
        {
            result = new(guid);
            return true;
        }
        result = default;
        return false;
    }

    public static bool operator ==(MaskedGuid left, MaskedGuid right) => left.Equals(right);

    public static bool operator !=(MaskedGuid left, MaskedGuid right) => !left.Equals(right);

    public static bool operator <(MaskedGuid left, MaskedGuid right) => left.CompareTo(right) < 0;

    public static bool operator <=(MaskedGuid left, MaskedGuid right) => left.CompareTo(right) <= 0;

    public static bool operator >(MaskedGuid left, MaskedGuid right) => left.CompareTo(right) > 0;

    public static bool operator >=(MaskedGuid left, MaskedGuid right) => left.CompareTo(right) >= 0;
}
