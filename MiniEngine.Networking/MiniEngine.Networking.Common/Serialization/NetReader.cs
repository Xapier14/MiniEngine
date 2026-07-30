using System.Buffers.Binary;
using System.Text;

namespace MiniEngine.Networking.Common.Serialization;

/// <summary>
/// Reads back what <see cref="NetWriter"/> wrote, in the same order. Running off the end
/// of the buffer throws rather than returning junk, so a truncated or misaligned packet
/// fails at the point of damage instead of quietly producing a wrong world.
/// </summary>
public ref struct NetReader
{
    private readonly ReadOnlySpan<byte> _buffer;
    private int _position;

    public NetReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    /// <summary>Bytes consumed so far.</summary>
    public readonly int Position => _position;

    /// <summary>Bytes left unread.</summary>
    public readonly int Remaining => _buffer.Length - _position;

    private ReadOnlySpan<byte> Take(int count)
    {
        if (count > _buffer.Length - _position)
            throw new InvalidOperationException(
                $"NetReader ran past the end of the buffer: needed {count} more byte(s), {_buffer.Length - _position} left.");
        var slice = _buffer.Slice(_position, count);
        _position += count;
        return slice;
    }

    public byte ReadByte()
        => Take(1)[0];

    public bool ReadBool()
        => ReadByte() != 0;

    public short ReadInt16()
        => BinaryPrimitives.ReadInt16LittleEndian(Take(sizeof(short)));

    public ushort ReadUInt16()
        => BinaryPrimitives.ReadUInt16LittleEndian(Take(sizeof(ushort)));

    public int ReadInt32()
        => BinaryPrimitives.ReadInt32LittleEndian(Take(sizeof(int)));

    public uint ReadUInt32()
        => BinaryPrimitives.ReadUInt32LittleEndian(Take(sizeof(uint)));

    public long ReadInt64()
        => BinaryPrimitives.ReadInt64LittleEndian(Take(sizeof(long)));

    public ulong ReadUInt64()
        => BinaryPrimitives.ReadUInt64LittleEndian(Take(sizeof(ulong)));

    public float ReadSingle()
        => BinaryPrimitives.ReadSingleLittleEndian(Take(sizeof(float)));

    public double ReadDouble()
        => BinaryPrimitives.ReadDoubleLittleEndian(Take(sizeof(double)));

    public string? ReadString()
    {
        var byteCount = ReadInt32();
        if (byteCount < 0)
            return null;
        return byteCount == 0 ? string.Empty : Encoding.UTF8.GetString(Take(byteCount));
    }
}
