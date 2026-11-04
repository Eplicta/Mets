using System;
using System.IO;
using System.IO.Compression;

namespace Eplicta.Mets;

public interface IArchiveStream : IDisposable
{
	Stream Stream { get; }
    byte[] ToArray();
}

public record ArchiveStream : IArchiveStream
{
    private readonly MemoryStream _memoryStream;
    private readonly ZipArchive _zipArchive;

    public ArchiveStream(MemoryStream memoryStream, ZipArchive zipArchive = default)
	{
        _memoryStream = memoryStream;
        _zipArchive = zipArchive;
    }

    public Stream Stream => _memoryStream;

    public void Dispose()
    {
        _zipArchive?.Dispose();
        _memoryStream?.Dispose();
    }

    public byte[] ToArray() => _memoryStream.ToArray();
}
