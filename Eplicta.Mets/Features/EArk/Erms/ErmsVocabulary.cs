using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// The term ERMS spells for a vocabulary value, taken from its <see cref="XmlEnumAttribute"/>.
/// </summary>
internal static class ErmsVocabulary
{
    private static readonly ConcurrentDictionary<Enum, string> Terms = new();

    public static string Term(Enum value)
    {
        return Terms.GetOrAdd(value, x => x.GetType().GetField(x.ToString())?.GetCustomAttribute<XmlEnumAttribute>()?.Name ?? throw new InvalidOperationException($"{x.GetType().Name}.{x} has no ERMS term."));
    }
}
