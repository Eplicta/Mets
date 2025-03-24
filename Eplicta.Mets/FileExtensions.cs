using System.Linq;
using MimeTypes;

namespace Eplicta.Mets;

public static class FileExtensions
{
    public static string GetMimeType(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        var type = fileName.Split('.').Last();
        var mimeType = MimeTypeMap.GetMimeType(type);
        return mimeType;
    }
}