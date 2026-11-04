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
    //private readonly TarOutputStream _tarOutputStream;
    private readonly ZipArchive _zipArchive;

    internal ArchiveStream(MemoryStream memoryStream, ZipArchive zipArchive = default) // TarOutputStream tarOutputStream = default, 
	{
        _memoryStream = memoryStream;
        //_tarOutputStream = tarOutputStream;
        _zipArchive = zipArchive;
    }

    //public static implicit operator MemoryStream(ArchiveStream archive) => archive._memoryStream;

    public Stream Stream => _memoryStream;

    public void Dispose()
    {
        //_tarOutputStream?.Dispose();
        _zipArchive?.Dispose();
        _memoryStream?.Dispose();
    }
    //public Task CopyToAsync(Stream stream) => Stream.CopyToAsync(stream);

    public byte[] ToArray() => _memoryStream.ToArray();
}