using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// A file whose bytes are read from a stream when the package is written, such as a file kept in blob storage.
/// Written by <see cref="Erms.ErmsPackageWriter"/>, which verifies the bytes against <see cref="EArkFile.Checksum"/>;
/// <see cref="EArkPackageWriter"/> does not read streams.
/// </summary>
public record EArkStreamedFile : EArkFile
{
    public Func<CancellationToken, Task<Stream>> Open { get; init; }
}
