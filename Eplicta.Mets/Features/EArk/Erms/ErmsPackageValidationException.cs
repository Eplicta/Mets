using System;
using System.Collections.Generic;
using System.Linq;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>An ERMS package whose own XML does not validate against the schemas it carries, and so was not written.</summary>
public class ErmsPackageValidationException : InvalidOperationException
{
    private const int FailuresInMessage = 10;

    public ErmsPackageValidationException(IReadOnlyList<string> failures)
        : base($"The package was not written, because its XML does not validate: {string.Join(" | ", (failures ?? []).Take(FailuresInMessage))}")
    {
        Failures = failures ?? [];
    }

    /// <summary>Every failure, each prefixed with the file it was found in.</summary>
    public IReadOnlyList<string> Failures { get; }
}
