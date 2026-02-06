using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

public record ValidatorResult
{
    public required SeverityType Severity { get; init; }
    public string Information { get; init; }
    public XmlValidatorResult XmlReslut { get; init; }
}