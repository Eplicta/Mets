using System;
using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Premis;

/// <summary>
/// Something done to the package or its files, such as its creation or a fixity check, with its outcome.
/// </summary>
public record PremisEvent
{
    public PremisIdentifier Identifier { get; init; }

    /// <summary>A term from the Library of Congress event type vocabulary, such as "fixity check".</summary>
    public string EventType { get; init; }

    public DateTime DateTime { get; init; }
    public string Detail { get; init; }
    public string Outcome { get; init; }
    public string OutcomeDetailNote { get; init; }
    public IReadOnlyList<PremisLinkingAgent> LinkingAgents { get; init; } = [];
    public IReadOnlyList<PremisIdentifier> LinkingObjects { get; init; } = [];
}
