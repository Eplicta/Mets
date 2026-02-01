namespace Eplicta.Mets.Entities;

public class MetsSchema : Enumeration<MetsSchema, MetsSchema>, IEnumerationItem
{
    public static MetsSchema Default => new("mets.xsd");
    public static MetsSchema KB => new("eARD_Paket_FGS-PUBL_mets.xsd");
    public static MetsSchema Riksarkivet => new("CSPackageMETS.xsd");

    private MetsSchema(string name)
    {
        Name = name;
    }

    public string Name { get; }
}