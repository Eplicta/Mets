﻿namespace Eplicta.Mets.Entities;

public class ModsVersion : Enumeration<ModsVersion, ModsVersion>, IEnumerationItem
{
    public static ModsVersion Mods_3_0 => new("mods-3-0");
    public static ModsVersion Mods_3_1 => new("mods-3-1");
    public static ModsVersion Mods_3_2 => new("mods-3-2");
    public static ModsVersion Mods_3_3 => new("mods-3-3");
    public static ModsVersion Mods_3_4 => new("mods-3-4");
    public static ModsVersion Mods_3_5 => new("mods-3-5");
    public static ModsVersion Mods_3_6 => new("mods-3-6");
    public static ModsVersion Mods_3_7 => new("mods-3-7");
    public static ModsVersion ModsFgsPubl_1_0 => new("MODS_enligt_FGS-PUBL_xml1_0");
    //public static Version ModsFgsPubl_1_1 = new("MODS_enligt_FGS-PUBL_xml1_1");

    private ModsVersion(string name)
    {
        Name = name;
    }

    public string Name { get; }
}