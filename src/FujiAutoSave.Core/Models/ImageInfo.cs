using FujiAutoSave.Core.Enums;

namespace FujiAutoSave.Core.Models;

public sealed class ImageInfo
{
    public required string FileName { get; set; }

    public uint StorageId { get; set; }

    public ImageFormat Format { get; set; }

    public ushort Protection { get; set; }

    public uint MaxPartialSize { get; set; }

    public uint CompressedSize { get; set; }

    public uint Width { get; set; }

    public uint Height { get; set; }

    public uint BitDepth { get; set; }

    public uint ParentObject { get; set; }

    public required DateTimeOffset CreateTime { get; set; }
}
