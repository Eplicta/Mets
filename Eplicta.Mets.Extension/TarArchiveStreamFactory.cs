using System.Text;
using Eplicta.Mets.Entities;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.IO;

namespace Eplicta.Mets;

public class TarArchiveStreamFactory : IArchiveStreamFactory
{
	private static readonly RecyclableMemoryStreamManager _recyclableMsManager = new();

	public IArchiveStream GetArchiveStream(MetsData metsData, string xmlString, string metsFileName)
	{
		var compressedFileStream = _recyclableMsManager.GetStream();
		var tarOutputStream = new TarOutputStream(compressedFileStream, Encoding.UTF8);
		var archiveStream = new ArchiveStream(compressedFileStream);

		foreach (var source in metsData.Sources ?? [])
		{
			throw new NotImplementedException("Stream resources from sources has not yet been implemented for Tar-archives.");
		}

		foreach (var resource in metsData.Files ?? [])
		{
			AddFile(tarOutputStream, $"{resource.FileName}", resource.Data);
		}

		AddFile(tarOutputStream, metsFileName, xmlString);

		tarOutputStream.Finish();

		return archiveStream;
	}

	private static void AddFile(TarOutputStream tarStream, string entryName, string data)
	{
		var bytes = Encoding.UTF8.GetBytes(data);
		AddFile(tarStream, entryName, bytes);
	}

	private static void AddFile(TarOutputStream tarStream, string entryName, byte[] data)
	{
		var tarEntry = new TarEntry(new TarHeader
		{
			Size = data.Length,
			Name = entryName
		});

		tarStream.PutNextEntry(tarEntry);
		tarStream.Write(data, 0, data.Length);
		tarStream.CloseEntry();
	}
}
