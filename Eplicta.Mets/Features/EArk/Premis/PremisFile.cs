using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// A file object: its identity, fixity, size, format, original name and where it lies in the package.
/// </summary>
public record PremisFile
{
    public PremisIdentifier Identifier { get; init; }
    public IReadOnlyList<PremisFixity> Fixities { get; init; } = [];
    public long? Size { get; init; }

    /// <summary>The format by name, such as a media type; PREMIS requires a format on every file.</summary>
    public string FormatName { get; init; }

    public string FormatVersion { get; init; }

    /// <summary>A PRONOM identifier, such as fmt/276, when the format has been identified against the registry.</summary>
    public string FormatRegistryKey { get; init; }

    public string FormatRegistryName { get; init; } = "PRONOM";
    public string OriginalName { get; init; }

    /// <summary>The file's path in the package, relative to the package root.</summary>
    public string ContentLocation { get; init; }

    public string ContentLocationType { get; init; } = "URI";
    public IReadOnlyList<PremisIdentifier> LinkingEventIdentifiers { get; init; } = [];
}
