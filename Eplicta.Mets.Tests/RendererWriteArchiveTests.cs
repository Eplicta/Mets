using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using AutoFixture;
using Eplicta.Mets.Entities;
using Eplicta.Mets.Tests.Helpers;
using FluentAssertions;
using ICSharpCode.SharpZipLib.Tar;
using Xunit;

namespace Eplicta.Mets.Tests;

public class RendererWriteArchiveTests
{
    private const long LargeSourceSize = 64L * 1024 * 1024;
    private const long AllocationBudget = 16L * 1024 * 1024;
    private const string MetsFileName = "sip.xml";
    private const string SourceName = "video.mp4";

    [Fact]
    public void WriteArchive_WhenFormatIsTar_ShouldWriteACompleteArchive()
    {
        var metsData = new Fixture().Build<MetsData>().Without(x => x.Sources).Create();
        var sut = new Renderer(metsData);
        using var target = new MemoryStream();

        sut.WriteArchive(target, ArchiveFormat.Tar, MetsFileName);

        var entries = ReadTar(target.ToArray());
        entries.Keys.Should().BeEquivalentTo(metsData.Files.Select(x => x.FileName).Append(MetsFileName));
        foreach (var file in metsData.Files) entries[file.FileName].Should().Equal(file.Data);
    }

    [Fact]
    public void WriteArchive_WhenSourceIsLarge_ShouldWriteTheArchiveToTheTargetInsteadOfMemory()
    {
        var sut = new Renderer(new MetsData { Sources = [new StreamFileSource { Name = SourceName, Size = LargeSourceSize, Stream = new IncompressibleStream(LargeSourceSize) }] });
        using var target = new FileStream(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 81920, FileOptions.DeleteOnClose);

        var before = GC.GetAllocatedBytesForCurrentThread();
        sut.WriteArchive(target, ArchiveFormat.Zip, MetsFileName);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        target.Length.Should().BeGreaterThan(LargeSourceSize);
        allocated.Should().BeLessThan(AllocationBudget);
    }

    [Fact]
    public void WriteArchive_WhenTargetIsGiven_ShouldLeaveItOpenAtTheEndOfTheArchive()
    {
        var sut = new Renderer(new MetsData { Sources = [new StreamFileSource { Name = SourceName, Stream = new MemoryStream([1, 2, 3]) }] });
        using var target = new MemoryStream();

        sut.WriteArchive(target, ArchiveFormat.Zip, MetsFileName);

        target.CanWrite.Should().BeTrue();
        target.Position.Should().Be(target.Length);
    }

    [Fact]
    public void WriteArchive_WhenTargetIsNull_ShouldThrow()
    {
        var sut = new Renderer(new MetsData());

        var act = () => sut.WriteArchive(null, ArchiveFormat.Zip);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WriteArchive_WhenWritingAZip_ShouldHoldTheSameEntriesAsGetArchiveStream()
    {
        var content = Enumerable.Range(0, 50_000).Select(x => (byte)(x % 253)).ToArray();
        var written = new Renderer(new MetsData { Sources = [new StreamFileSource { Name = SourceName, Stream = new MemoryStream(content) }] });
        var returned = new Renderer(new MetsData { Sources = [new StreamFileSource { Name = SourceName, Stream = new MemoryStream(content) }] });
        using var target = new MemoryStream();

        written.WriteArchive(target, ArchiveFormat.Zip, MetsFileName);
        using var archiveStream = returned.GetArchiveStream(ArchiveFormat.Zip, MetsFileName);

        var writtenEntries = ReadZip(target.ToArray());
        var returnedEntries = ReadZip(archiveStream.ToArray());
        writtenEntries.Keys.Should().BeEquivalentTo(returnedEntries.Keys);
        writtenEntries[SourceName].Should().Equal(content);
        returnedEntries[SourceName].Should().Equal(content);
    }

    private static Dictionary<string, byte[]> ReadZip(byte[] archive)
    {
        using var zip = new ZipArchive(new MemoryStream(archive), ZipArchiveMode.Read);
        return zip.Entries.ToDictionary(x => x.FullName, x =>
        {
            using var entryStream = x.Open();
            using var content = new MemoryStream();
            entryStream.CopyTo(content);
            return content.ToArray();
        });
    }

    private static Dictionary<string, byte[]> ReadTar(byte[] archive)
    {
        var entries = new Dictionary<string, byte[]>();
        using var tar = new TarInputStream(new MemoryStream(archive), System.Text.Encoding.UTF8);
        while (tar.GetNextEntry() is { } entry)
        {
            using var content = new MemoryStream();
            tar.CopyEntryContents(content);
            entries[entry.Name] = content.ToArray();
        }

        return entries;
    }
}
