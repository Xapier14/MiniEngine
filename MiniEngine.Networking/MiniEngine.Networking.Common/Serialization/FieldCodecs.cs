using MiniEngine.Networking.Common.Serialization;

namespace MiniEngine.Networking.Common.Replication;

/// <summary>
/// Knows how to put one field type on the wire. Kept separate from the field accessor so
/// the same encoding can be reused wherever a type appears, and so changing an encoding
/// changes exactly one place.
/// </summary>
public interface IFieldCodec<T>
{
    /// <summary>
    /// Stable tag for this encoding, used in the schema hash. Change it whenever the bytes
    /// change, so an old client is rejected instead of misreading them.
    /// </summary>
    string WireTypeName { get; }

    void Write(ref NetWriter writer, T value);
    T Read(ref NetReader reader);
}

internal sealed class BoolCodec : IFieldCodec<bool>
{
    public string WireTypeName => "bool";
    public void Write(ref NetWriter writer, bool value) => writer.WriteBool(value);
    public bool Read(ref NetReader reader) => reader.ReadBool();
}

internal sealed class ByteCodec : IFieldCodec<byte>
{
    public string WireTypeName => "u8";
    public void Write(ref NetWriter writer, byte value) => writer.WriteByte(value);
    public byte Read(ref NetReader reader) => reader.ReadByte();
}

internal sealed class Int32Codec : IFieldCodec<int>
{
    public string WireTypeName => "i32";
    public void Write(ref NetWriter writer, int value) => writer.WriteInt32(value);
    public int Read(ref NetReader reader) => reader.ReadInt32();
}

internal sealed class UInt32Codec : IFieldCodec<uint>
{
    public string WireTypeName => "u32";
    public void Write(ref NetWriter writer, uint value) => writer.WriteUInt32(value);
    public uint Read(ref NetReader reader) => reader.ReadUInt32();
}

internal sealed class Int64Codec : IFieldCodec<long>
{
    public string WireTypeName => "i64";
    public void Write(ref NetWriter writer, long value) => writer.WriteInt64(value);
    public long Read(ref NetReader reader) => reader.ReadInt64();
}

internal sealed class UInt64Codec : IFieldCodec<ulong>
{
    public string WireTypeName => "u64";
    public void Write(ref NetWriter writer, ulong value) => writer.WriteUInt64(value);
    public ulong Read(ref NetReader reader) => reader.ReadUInt64();
}

internal sealed class DoubleCodec : IFieldCodec<double>
{
    public string WireTypeName => "f64";
    public void Write(ref NetWriter writer, double value) => writer.WriteDouble(value);
    public double Read(ref NetReader reader) => reader.ReadDouble();
}

internal sealed class StringCodec : IFieldCodec<string?>
{
    public string WireTypeName => "str";
    public void Write(ref NetWriter writer, string? value) => writer.WriteString(value);
    public string? Read(ref NetReader reader) => reader.ReadString();
}

/// <summary>
/// A float, optionally rounded to a fixed step before sending. Quantising is lossy by
/// design: positions rarely need all 24 bits of mantissa, and the integer form is what
/// later delta compression will pack down.
/// </summary>
internal sealed class SingleCodec(float quantize) : IFieldCodec<float>
{
    public string WireTypeName => quantize > 0f ? $"f32q{quantize:R}" : "f32";

    public void Write(ref NetWriter writer, float value)
    {
        if (quantize > 0f)
            writer.WriteInt32((int)MathF.Round(value / quantize));
        else
            writer.WriteSingle(value);
    }

    public float Read(ref NetReader reader)
        => quantize > 0f ? reader.ReadInt32() * quantize : reader.ReadSingle();
}

internal sealed class Vector2FCodec(float quantize) : IFieldCodec<Vector2F>
{
    private readonly SingleCodec _component = new(quantize);

    public string WireTypeName => $"vec2<{_component.WireTypeName}>";

    public void Write(ref NetWriter writer, Vector2F value)
    {
        _component.Write(ref writer, value.X);
        _component.Write(ref writer, value.Y);
    }

    public Vector2F Read(ref NetReader reader)
        => new(_component.Read(ref reader), _component.Read(ref reader));
}

internal sealed class Vector2Codec : IFieldCodec<Vector2>
{
    public string WireTypeName => "vec2i";

    public void Write(ref NetWriter writer, Vector2 value)
    {
        writer.WriteInt32(value.X);
        writer.WriteInt32(value.Y);
    }

    public Vector2 Read(ref NetReader reader)
        => new(reader.ReadInt32(), reader.ReadInt32());
}

internal sealed class SizeCodec : IFieldCodec<Size>
{
    public string WireTypeName => "size";

    public void Write(ref NetWriter writer, Size value)
    {
        writer.WriteInt32(value.Width);
        writer.WriteInt32(value.Height);
    }

    public Size Read(ref NetReader reader)
        => new(reader.ReadInt32(), reader.ReadInt32());
}

internal sealed class SizeFCodec(float quantize) : IFieldCodec<SizeF>
{
    private readonly SingleCodec _component = new(quantize);

    public string WireTypeName => $"sizef<{_component.WireTypeName}>";

    public void Write(ref NetWriter writer, SizeF value)
    {
        _component.Write(ref writer, value.Width);
        _component.Write(ref writer, value.Height);
    }

    public SizeF Read(ref NetReader reader)
        => new(_component.Read(ref reader), _component.Read(ref reader));
}

internal sealed class ColorCodec : IFieldCodec<Color>
{
    public string WireTypeName => "rgba8";

    public void Write(ref NetWriter writer, Color value)
    {
        writer.WriteByte(value.R);
        writer.WriteByte(value.G);
        writer.WriteByte(value.B);
        writer.WriteByte(value.A);
    }

    public Color Read(ref NetReader reader)
        => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
}

/// <summary>
/// Any enum, as its underlying integer widened to 32 bits. Boxing through Convert is
/// acceptable here: enums are rare in replicated state, and correctness across every
/// underlying type is worth more than avoiding one allocation per field.
/// </summary>
internal sealed class EnumCodec<T> : IFieldCodec<T> where T : struct, Enum
{
    public string WireTypeName => "enum32";

    public void Write(ref NetWriter writer, T value)
        => writer.WriteInt32(Convert.ToInt32(value));

    public T Read(ref NetReader reader)
        => (T)Enum.ToObject(typeof(T), reader.ReadInt32());
}

/// <summary>
/// Picks the encoding for a field type. A type with no entry here cannot be replicated,
/// and says so at registration rather than at the first snapshot.
/// </summary>
internal static class FieldCodecs
{
    public static IFieldCodec<T> For<T>(float quantize)
    {
        var type = typeof(T);

        if (type.IsEnum)
            return (IFieldCodec<T>)Activator.CreateInstance(typeof(EnumCodec<>).MakeGenericType(type))!;

        object? codec = type switch
        {
            _ when type == typeof(bool) => new BoolCodec(),
            _ when type == typeof(byte) => new ByteCodec(),
            _ when type == typeof(int) => new Int32Codec(),
            _ when type == typeof(uint) => new UInt32Codec(),
            _ when type == typeof(long) => new Int64Codec(),
            _ when type == typeof(ulong) => new UInt64Codec(),
            _ when type == typeof(float) => new SingleCodec(quantize),
            _ when type == typeof(double) => new DoubleCodec(),
            _ when type == typeof(string) => new StringCodec(),
            _ when type == typeof(Vector2F) => new Vector2FCodec(quantize),
            _ when type == typeof(Vector2) => new Vector2Codec(),
            _ when type == typeof(Size) => new SizeCodec(),
            _ when type == typeof(SizeF) => new SizeFCodec(quantize),
            _ when type == typeof(Color) => new ColorCodec(),
            _ => null,
        };

        if (codec is null)
            throw new NotSupportedException(
                $"No replication encoding for {type.Name}. Replicated fields must be a primitive, " +
                $"an enum, a string, or one of the engine's value types.");

        return (IFieldCodec<T>)codec;
    }
}
