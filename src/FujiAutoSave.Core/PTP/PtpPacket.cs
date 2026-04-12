using System.Buffers.Binary;
using FujiAutoSave.Core.Enums;

namespace FujiAutoSave.Core.PTP;

public abstract class PtpPacket
{
    public const int HeaderSize = 12;

    public uint Length { get; set; }

    public PtpPacketType Type { get; set; }

    public ushort Code { get; set; }

    public uint TransactionId { get; set; }

    public byte[] ToByteArray(uint[]? parameters = null, byte[]? payload = null)
    {
        var paramSize = parameters?.Length ?? 0;
        var payloadSize = payload?.Length ?? 0;
        var totalSize = HeaderSize + (paramSize * 4) + payloadSize;
        var buffer = new byte[totalSize];
        var offset = 0;
        Length = (uint)totalSize;

        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), Length);
        offset += 4;

        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), (ushort)Type);
        offset += 2;

        BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), Code);
        offset += 2;

        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), TransactionId);
        offset += 4;

        if (parameters != null)
        {
            foreach (var item in parameters)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), item);
                offset += 4;
            }
        }

        if (payload != null)
        {
            Array.Copy(payload, 0, buffer, offset, payloadSize);
        }

        return buffer;
    }

    public static PtpPacket FromByteArray(byte[] buffer)
    {
        if (buffer.Length < 8)
        {
            throw new ArgumentException($"Buffer too small: {buffer.Length} bytes, expected at least 8 bytes", nameof(buffer));
        }

        var length = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(0));
        var type = (PtpPacketType)BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(4));
        var code = buffer.Length >= 10 ? BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(6)) : (ushort)0;
        var transactionId = buffer.Length >= 12 ? BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(8)) : 0;

        return type switch
        {
            PtpPacketType.Command => new PtpCommandPacket
            {
                Length = length,
                Type = type,
                Code = code,
                TransactionId = transactionId
            },
            PtpPacketType.Data => new PtpDataPacket
            {
                Length = length,
                Type = type,
                Code = code,
                TransactionId = transactionId
            },
            PtpPacketType.Response => new PtpResponsePacket
            {
                Length = length,
                Type = type,
                Code = code,
                TransactionId = transactionId
            },
            _ => throw new NotSupportedException()
        };
    }
}
