using System;
using System.Collections.Generic;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// One change to a case, when and by whom, with the case fields and record fields it changed.
/// </summary>
public record ErmsLogModification
{
    public string Number { get; init; }
    public DateTime Date { get; init; }
    public string Agent { get; init; }
    public IReadOnlyList<ErmsLogField> CaseFields { get; init; } = [];
    public IReadOnlyList<ErmsLogRecordChange> RecordChanges { get; init; } = [];
}
