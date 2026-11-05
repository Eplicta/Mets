using System.IO;
using System.IO.Compression;
using System.Text;

namespace Eplicta.Mets;

public static class ZipArchiveExtensions
{
	public static void AddFile(this ZipArchive zipArchive, string entryName, string data)
	{
		var bytes = Encoding.UTF8.GetBytes(data);
		AddFile(zipArchive, entryName, bytes);
	}

	public static void AddFile(this ZipArchive zipArchive, string entryName, byte[] data)
	{
		using var stream = new MemoryStream(data);
		AddFile(zipArchive, entryName, stream);
	}

	public static void AddFile(this ZipArchive zipArchive, string entryName, MemoryStream stream)
	{
		var entry = zipArchive.CreateEntry(entryName);
		using var originalFileStream = stream;
		using var zipEntryStream = entry.Open();
		originalFileStream.CopyTo(zipEntryStream);
	}
}
