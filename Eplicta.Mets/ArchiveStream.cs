using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

namespace Eplicta.Mets;

public record ArchiveStream : IDisposable
{
    private readonly MemoryStream _memoryStream;
    private readonly TarOutputStream _tarOutputStream;
    private readonly ZipArchive _zipArchive;

    internal ArchiveStream(MemoryStream memoryStream, TarOutputStream tarOutputStream = default, ZipArchive zipArchive = default)
    {
        _memoryStream = memoryStream;
        _tarOutputStream = tarOutputStream;
        _zipArchive = zipArchive;
    }

    public static implicit operator MemoryStream(ArchiveStream archive) => archive._memoryStream;

    public MemoryStream Stream => _memoryStream;

    public void Dispose()
    {
        _tarOutputStream?.Dispose();
        _zipArchive?.Dispose();
        _memoryStream?.Dispose();
    }

    public Task CopyToAsync(Stream stream)
    {
        return Stream.CopyToAsync(stream);
    }

    public byte[] ToArray()
    {
        return Stream.ToArray();
    }
}