namespace Eplicta.Mets.Entities;

public class Version : Enumeration<Version, Version>, IEnumerationItem
{
    internal const string Mods_3_5_Key = "mods-3-5";
    internal const string Mods_3_6_Key = "mods-3-6";
    internal const string Mods_3_7_Key = "mods-3-7";
    internal const string ModsFgsPubl_1_0_Key = "MODS_enligt_FGS-PUBL_xml1_0";
    internal const string ModsFgsPubl_1_1_Key = "MODS_enligt_FGS-PUBL_xml1_1";
    internal const string eARD_Paket_FGS_PUBL_mets_Key = "eARD_Paket_FGS-PUBL_mets";

    //public static Version Mods_3_0 = new("mods-3-0");
    //public static Version Mods_3_1 = new("mods-3-1");
    //public static Version Mods_3_2 = new("mods-3-2");
    //public static Version Mods_3_3 = new("mods-3-3");
    //public static Version Mods_3_4 = new("mods-3-4");
    public static Version Mods_3_5 = new(Mods_3_5_Key, "MODS 3.5");
    public static Version Mods_3_6 = new(Mods_3_6_Key, "MODS 3.6");
    public static Version Mods_3_7 = new(Mods_3_7_Key, "MODS 3.7");
    public static Version ModsFgsPubl_1_0 = new(ModsFgsPubl_1_0_Key, "MODS FGS-PUBL 1.0");
    //public static Version ModsFgsPubl_1_1 = new(ModsFgsPubl_1_1_Key, "MODS FGS-PUBL 1.1");
    //public static Version eARD_Paket_FGS_PUBL_mets = new(eARD_Paket_FGS_PUBL_mets_Key, "eARD FGS-PUBL 1.0");

    private Version(string key, string name)
    {
        Key = key;
        Name = name;
    }

    public string Key { get; }
    public string Name { get; }
}