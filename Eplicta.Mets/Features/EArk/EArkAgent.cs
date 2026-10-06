namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// A mets/metsHdr/agent entry. CSIP10-16 require one agent describing the software that created the package;
/// SIP15-20 require one describing the submitting organisation or individual.
/// </summary>
public record EArkAgent
{
    public enum ERole
    {
        Creator,
        Editor,
        Archivist,
        Preservation,
        Disseminator,
        Custodian,
        IpOwner,
        Other
    }

    public enum EType
    {
        Individual,
        Organization,
        Other
    }

    public enum ENoteType
    {
        SoftwareVersion,
        IdentificationCode
    }

    public ERole Role { get; init; }
    public EType Type { get; init; }
    public string OtherRole { get; init; }
    public string OtherType { get; init; }
    public string Name { get; init; }
    public string Note { get; init; }
    public ENoteType? NoteType { get; init; }

    /// <summary>
    /// The agent CSIP10-16 mandate, describing the tool that produced the package. Every attribute value is fixed.
    /// </summary>
    public static EArkAgent Software(string name, string version)
    {
        return new EArkAgent
        {
            Role = ERole.Creator,
            Type = EType.Other,
            OtherType = EArkConstants.SoftwareAgentOtherType,
            Name = name,
            Note = version,
            NoteType = ENoteType.SoftwareVersion
        };
    }

    /// <summary>
    /// The submitting agent SIP15-20 mandate.
    /// </summary>
    public static EArkAgent Submitter(string name, EType type = EType.Organization, string identificationCode = null)
    {
        return new EArkAgent
        {
            Role = ERole.Creator,
            Type = type,
            Name = name,
            Note = identificationCode,
            NoteType = identificationCode == null ? null : ENoteType.IdentificationCode
        };
    }

    /// <summary>
    /// The archival creator SIP9-14 describe, responsible for the archived material.
    /// </summary>
    public static EArkAgent Archivist(string name, EType type = EType.Organization, string identificationCode = null)
    {
        return new EArkAgent
        {
            Role = ERole.Archivist,
            Type = type,
            Name = name,
            Note = identificationCode,
            NoteType = identificationCode == null ? null : ENoteType.IdentificationCode
        };
    }
}
