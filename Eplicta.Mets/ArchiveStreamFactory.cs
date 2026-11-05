using System;
using System.IO.Compression;
using System.Security.Cryptography;
using Eplicta.Mets.Entities;
using Microsoft.IO;
using static Eplicta.Mets.Entities.MetsData;

namespace Eplicta.Mets;

public interface IArchiveStreamFactory
{
    IArchiveStream GetArchiveStream(MetsData metsData, string xml, string metsFileName);
}

public class DefaultZipArchiveStreamFactory : IArchiveStreamFactory
{
	private static readonly RecyclableMemoryStreamManager _recyclableMsManager = new();

	public IArchiveStream GetArchiveStream(MetsData metsData, string xmlString, string metsFileName)
    {
        var compressedFileStream = _recyclableMsManager.GetStream();
        using var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update, true);
        var archiveStream = new ArchiveStream(compressedFileStream, zipArchive);

        foreach (var source in metsData.Sources ?? [])
        {
            using var md5 = MD5.Create();
            var entry = zipArchive.CreateEntry(source.Name);

            using (var zipEntryStream = entry.Open())

            using (var hashingStream = new CryptoStream(zipEntryStream, md5, CryptoStreamMode.Write))
            {
                source.Stream.CopyTo(hashingStream);
            }

            source.Size ??= source.Stream.Length;

            var hash = BitConverter.ToString(md5.Hash!).Replace("-", "");
            if (string.IsNullOrEmpty(source.Id))
            {
                source.Id = $"ID{hash}";
            }

            if (string.IsNullOrEmpty(source.Checksum))
            {
                source.Checksum = hash;
                source.ChecksumType = EChecksumType.MD5;
            }
        }

        foreach (var resource in metsData.Files ?? [])
        {
			zipArchive.AddFile($"{resource.FileName}", resource.Data);
        }

		zipArchive.AddFile(metsFileName, xmlString);

        return archiveStream;
    }
}
