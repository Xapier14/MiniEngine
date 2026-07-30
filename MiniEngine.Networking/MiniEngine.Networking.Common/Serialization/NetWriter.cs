using System.Buffers.Binary;
using System.Text;

namespace MiniEngine.Networking.Common.Serialization;

/// <summary>
/// <para>Writes primitives into a caller-supplied buffer.</para>
/// <para>A ref struct over a <see cref="Span{T}"/> rather than a BinaryWriter over a
/// stream: a snapshot is built every tick, and this way building one allocates nothing.
/// Everything is little endian explicitly, so a big endian machine talks to a little
/// endian one without either noticing.</para>
/// </summary>
public ref struct NetWriter
{
    private readonly Span<byte> _buffer;
    private int _position;

    public NetWriter(Span<byte> buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    /// <summary>Bytes written so far.</summary>
    public readonly int Position => _position;

    /// <summary>Room left in the buffer.</summary>
    public readonly int Remaining => _buffer.Length - _position;

    /// <summary>The portion of the buffer that has been written.</summary>
    public readonly ReadOnlySpan<byte> Written => _buffer[.._position];

    private Span<byte> Take(int count)
    {
        if (count > _buffer.Length - _position)
            throw new InvalidOperationException(
                $"NetWriter ran out of buffer: needed {count} more byte(s), {_buffer.Length - _position} left.");
        var slice = _buffer.Slice(_position, count);
        _position += count;
        return slice;
    }

    public void WriteByte(byte value)
        => Take(1)[0] = value;

    public void WriteBool(bool value)
        => WriteByte(value ? (byte)1 : (byte)0);

    public void WriteInt16(short value)
        => BinaryPrimitives.WriteInt16LittleEndian(Take(sizeof(short)), value);

    public void WriteUInt16(ushort value)
        => BinaryPrimitives.WriteUInt16LittleEndian(Take(sizeof(ushort)), value);

    public void WriteInt32(int value)
        => BinaryPrimitives.WriteInt32LittleEndian(Take(sizeof(int)), value);

    public void WriteUInt32(uint value)
        => BinaryPrimitives.WriteUInt32LittleEndian(Take(sizeof(uint)), value);

    public void WriteInt64(long value)
        => BinaryPrimitives.WriteInt64LittleEndian(Take(sizeof(long)), value);

    public void WriteUInt64(ulong value)
        => BinaryPrimitives.WriteUInt64LittleEndian(Take(sizeof(ulong)), value);

    public void WriteSingle(float value)
        => BinaryPrimitives.WriteSingleLittleEndian(Take(sizeof(float)), value);

    public void WriteDouble(double value)
        => BinaryPrimitives.WriteDoubleLittleEndian(Take(sizeof(double)), value);

    /// <summary>
    /// Writes a UTF-8 string prefixed with its byte length. A null string is a length of
    /// -1, which is distinct from an empty one.
    /// </summary>
    public void WriteString(string? value)
    {
        if (value == null)
        {
            WriteInt32(-1);
            return;
        }

        var byteCount = Encoding.UTF8.GetByteCount(value);
        WriteInt32(byteCount);
        if (byteCount > 0)
            Encoding.UTF8.GetBytes(value, Take(byteCount));
    }
}
