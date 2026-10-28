using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A named value the ERMS vocabulary has no element for, such as a field an organisation added itself.
/// </summary>
public record ErmsOwnElement
{
    public string Name { get; init; }
    public string DataType { get; init; }
    public string Format { get; init; }
    public string Value { get; init; }
    public IReadOnlyList<ErmsOwnElement> Children { get; init; } = [];
}
