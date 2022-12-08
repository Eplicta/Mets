using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Eplicta.Mets.Entities;

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