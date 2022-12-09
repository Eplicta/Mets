namespace Eplicta.Mets.Entities;

public class ArchiveFormat : Enumeration<ArchiveFormat, ArchiveFormat>, IEnumerationItem
{
    public static ArchiveFormat Zip = new("Zip");
    public static ArchiveFormat Tar = new("Tar");

    public ArchiveFormat(string name)
    {
        Name = name;
    }

    public string Name { get; }

}