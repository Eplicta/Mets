using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests;

public class ZipArchiveMemoryTests
{
    private const long SourceSize = 64L * 1024 * 1024;
    private const long AllocationBudget = 16L * 1024 * 1024;
    private const string SourceName = "large.pdf";

    [Fact]
    public void GetArchiveStream_LargeSource_IsNotBufferedUncompressedInMemory()
    {
        var sut = new Renderer(new MetsData { Sources = [new StreamFileSource { Name = SourceName, Stream = new ZeroStream(SourceSize) }] });

        var before = GC.GetAllocatedBytesForCurrentThread();
        using var result = sut.GetArchiveStream(ArchiveFormat.Zip);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        allocated.Should().BeLessThan(AllocationBudget);
    }

    [Fact]
    public void GetArchiveStream_WithSource_ArchiveHoldsTheSourceAndTheMetsFile()
    {
        var content = Enumerable.Range(0, 100_000).Select(x => (byte)(x % 251)).ToArray();
        var source = new StreamFileSource { Name = SourceName, Stream = new MemoryStream(content) };
        var sut = new Renderer(new MetsData { Sources = [source] });

        using var result = sut.GetArchiveStream(ArchiveFormat.Zip, "mets.xml");
        using var archive = new ZipArchive(new MemoryStream(result.ToArray()), ZipArchiveMode.Read);

        archive.Entries.Select(x => x.FullName).Should().BeEquivalentTo(SourceName, "mets.xml");
        using var entryContent = new MemoryStream();
        using (var entryStream = archive.GetEntry(SourceName)!.Open()) entryStream.CopyTo(entryContent);
        entryContent.ToArray().Should().Equal(content);
        source.Checksum.Should().Be(Convert.ToHexString(MD5.HashData(content)));
        source.Size.Should().Be(content.Length);
    }
}
