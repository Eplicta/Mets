using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Eplicta.Mets.Entities;

public class Version : Enumeration<Version, Version>, IEnumerationItem
{
    public static Version Mods_3_0 = new("mods-3-0");
    public static Version Mods_3_1 = new("mods-3-1");
    public static Version Mods_3_2 = new("mods-3-2");
    public static Version Mods_3_3 = new("mods-3-3");
    public static Version Mods_3_4 = new("mods-3-4");
    public static Version Mods_3_5 = new("mods-3-5");
    public static Version Mods_3_6 = new("mods-3-6");
    public static Version Mods_3_7 = new("mods-3-7");
    public static Version ModsFgsPubl_1_0 = new("MODS_enligt_FGS-PUBL_xml1_0");
    public static Version ModsFgsPubl_1_1 = new("MODS_enligt_FGS-PUBL_xml1_1");

    private Version(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public interface IEnumerationItem
{
    public string Name { get; }
}

public abstract class Enumeration<TContainer, TItem>
    where TItem : IEnumerationItem
{
    private static Dictionary<string, TItem> _items;

    static Enumeration()
    {
        var items = GetItems();
        Debug.WriteLine($"Loaded {items.Values.Count} items for type {typeof(TItem).Name}.");
    }

    public static IEnumerable<TItem> All()
    {
        return GetItems().Values;
    }

    public static IEnumerable<TItem> Where(Func<TItem, bool> predicate)
    {
        return GetItems().Values.Where(predicate);
    }

    public static TItem Single(Func<TItem, bool> predicate)
    {
        return GetItems().Values.Single(predicate);
    }

    public static TItem Single(string name)
    {
        return GetItems().TryGetValue(name, out var item) ? item : throw new InvalidOperationException($"Cannot find an item with name '{name}' for type '{typeof(TItem).Name}'.");
    }

    private static IDictionary<string, TItem> GetItems()
    {
        var items = _items ??= typeof(TContainer)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<TItem>()
            .ToDictionary(x => x.Name, x => x);

        return items;
    }
}