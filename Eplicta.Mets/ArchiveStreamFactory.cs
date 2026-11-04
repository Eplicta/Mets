using System;
using System.IO;
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
// ArchiveFormat archiveFormat, 

public class TarArchiveStreamFactory : IArchiveStreamFactory
{
	public IArchiveStream GetArchiveStream(MetsData metsData, string xml, string metsFileName)
		=> throw new NotImplementedException();

	//private IArchiveStream GetTarArchiveStream(string xmlString, string metsFileName)
	//{
	//    var compressedFileStream = _recyclableMsManager.GetStream();
	//    var tarOutputStream = new TarOutputStream(compressedFileStream, Encoding.UTF8);
	//    var archiveStream = new ArchiveStream(compressedFileStream); // tarOutputStream

	//    foreach (var source in _metsData.Sources ?? [])
	//    {
	//        throw new NotImplementedException("Stream resources from sources has not yet been implemented for Tar-archives.");
	//    }

	//    foreach (var resource in _metsData.Files ?? [])
	//    {
	//        AddFile(tarOutputStream, $"{resource.FileName}", resource.Data);
	//    }

	//    AddFile(tarOutputStream, metsFileName, xmlString);

	//    tarOutputStream.Finish();

	//    return archiveStream;
	//}

	//private static void AddFile(TarOutputStream tarStream, string entryName, string data)
	//{
	//    var bytes = Encoding.UTF8.GetBytes(data);
	//    AddFile(tarStream, entryName, bytes);
	//}
	//private static void AddFile(TarOutputStream tarStream, string entryName, byte[] data)
	//{
	//    var tarEntry = new TarEntry(new TarHeader
	//    {
	//        Size = data.Length,
	//        Name = entryName
	//    });

	//    tarStream.PutNextEntry(tarEntry);
	//    tarStream.Write(data, 0, data.Length);
	//    tarStream.CloseEntry();
	//}
}

public class DefaultZipArchiveStreamFactory : IArchiveStreamFactory
{
	private static readonly RecyclableMemoryStreamManager _recyclableMsManager = new();

	//private readonly MetsData _metsData;
	//public DefaultArchiveStreamFactory(MetsData metsData)
	//{
	//	_metsData = metsData;
	//}

	public IArchiveStream GetArchiveStream(MetsData metsData, string xml, string metsFileName)
    {
        //var archive = archiveFormat switch
        //{
        //	ArchiveFormat.Zip => GetZipArchiveStream(metsData, xml, metsFileName),
        //	//ArchiveFormat.Tar => GetTarArchiveStream(xml, metsFileName),
        //	_ => throw new ArgumentOutOfRangeException(nameof(archiveFormat), archiveFormat, null)
        //};
        var archive = GetZipArchiveStream(metsData, xml, metsFileName);
		archive.Stream.Seek(0, SeekOrigin.Begin);
		return archive;
	}

	private IArchiveStream GetZipArchiveStream(MetsData metsData, string xmlString, string metsFileName)
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
