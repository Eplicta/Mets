using System;
using System.Collections.Generic;
using System.Linq;

namespace Eplicta.Mets.Features.EArk;

/// <summary>
/// Accumulates the parts of an E-ARK package and validates the requirements CSIP and SIP make mandatory.
/// </summary>
public class EArkPackageBuilder
{
    private readonly List<EArkAgent> _agents = [];
    private readonly List<EArkAltRecordId> _altRecordIds = [];
    private readonly List<EArkMetadataReference> _descriptiveMetadata = [];
    private readonly List<EArkMetadataReference> _preservationMetadata = [];
    private readonly List<EArkFile> _documentationFiles = [];
    private readonly List<EArkFile> _schemaFiles = [];
    private readonly List<EArkFile> _representationFiles = [];
    private string _objId;
    private string _label;
    private string _contentCategory = EArkConstants.ContentCategory.WebArchives;
    private string _otherContentCategory;
    private EArkPackage.EContentInformationType _contentInformationType = EArkPackage.EContentInformationType.OTHER;
    private string _otherContentInformationType;
    private EArkPackage.ERecordStatus _recordStatus = EArkPackage.ERecordStatus.New;
    private string _representationName = EArkConstants.DefaultRepresentationName;
    private DateTime? _createDate;
    private DateTime? _lastModDate;
    private AdditionalPackageInfo _additionalPackageInfo;

    public EArkPackage Build()
    {
        if (string.IsNullOrWhiteSpace(_objId)) throw new InvalidOperationException("The required attribute ObjId is missing.");
        if (!_agents.Any(IsSoftwareAgent)) throw new InvalidOperationException("A software agent is required. CSIP10-16 mandate an agent describing the tool that created the package.");
        if (!_agents.Any(IsSubmittingAgent)) throw new InvalidOperationException("A submitting agent is required. SIP15-20 mandate an agent describing who submits the package.");

        return new EArkPackage
        {
            ObjId = _objId,
            Label = _label,
            ContentCategory = _contentCategory,
            OtherContentCategory = _otherContentCategory,
            ContentInformationType = _contentInformationType,
            OtherContentInformationType = _otherContentInformationType,
            RecordStatus = _recordStatus,
            RepresentationName = _representationName,
            CreateDate = _createDate ?? DateTime.UtcNow,
            LastModDate = _lastModDate,
            Agents = _agents.ToArray(),
            AltRecordIds = _altRecordIds.ToArray(),
            DescriptiveMetadata = _descriptiveMetadata.Select(WithId).ToArray(),
            PreservationMetadata = _preservationMetadata.Select(WithId).ToArray(),
            DocumentationFiles = _documentationFiles.Select(WithId).ToArray(),
            SchemaFiles = _schemaFiles.Select(WithId).ToArray(),
            RepresentationFiles = _representationFiles.Select(WithId).ToArray(),
            AdditionalPackageInfo = _additionalPackageInfo
        };
    }

    public EArkPackageBuilder SetObjId(string objId)
    {
        _objId = objId;
        return this;
    }

    public EArkPackageBuilder SetLabel(string label)
    {
        _label = label;
        return this;
    }

    public EArkPackageBuilder SetContentCategory(string contentCategory, string otherContentCategory = null)
    {
        _contentCategory = contentCategory;
        _otherContentCategory = otherContentCategory;
        return this;
    }

    public EArkPackageBuilder SetContentInformationType(EArkPackage.EContentInformationType contentInformationType, string otherContentInformationType = null)
    {
        _contentInformationType = contentInformationType;
        _otherContentInformationType = otherContentInformationType;
        return this;
    }

    public EArkPackageBuilder SetRecordStatus(EArkPackage.ERecordStatus recordStatus)
    {
        _recordStatus = recordStatus;
        return this;
    }

    public EArkPackageBuilder SetRepresentationName(string representationName)
    {
        _representationName = representationName;
        return this;
    }

    public EArkPackageBuilder SetCreateDate(DateTime createDate)
    {
        _createDate = createDate;
        return this;
    }

    public EArkPackageBuilder SetLastModDate(DateTime lastModDate)
    {
        _lastModDate = lastModDate;
        return this;
    }

    public EArkPackageBuilder SetSoftware(string name, string version)
    {
        _agents.RemoveAll(IsSoftwareAgent);
        _agents.Add(EArkAgent.Software(name, version));
        return this;
    }

    public EArkPackageBuilder AddAgent(EArkAgent agent)
    {
        _agents.Add(agent);
        return this;
    }

    public EArkPackageBuilder AddAltRecordId(EArkAltRecordId altRecordId)
    {
        _altRecordIds.Add(altRecordId);
        return this;
    }

    public EArkPackageBuilder AddDescriptiveMetadata(EArkMetadataReference reference)
    {
        _descriptiveMetadata.Add(reference);
        return this;
    }

    public EArkPackageBuilder AddPreservationMetadata(EArkMetadataReference reference)
    {
        _preservationMetadata.Add(reference);
        return this;
    }

    public EArkPackageBuilder AddDocumentationFile(EArkFile file)
    {
        _documentationFiles.Add(file);
        return this;
    }

    public EArkPackageBuilder AddSchemaFile(EArkFile file)
    {
        _schemaFiles.Add(file);
        return this;
    }

    public EArkPackageBuilder AddRepresentationFile(EArkFile file)
    {
        _representationFiles.Add(file);
        return this;
    }

    public EArkPackageBuilder SetAdditionalPackageInfo(AdditionalPackageInfo additionalPackageInfo)
    {
        _additionalPackageInfo = additionalPackageInfo;
        return this;
    }

    internal static string NewId()
    {
        return $"{EArkConstants.IdPrefix}{Guid.NewGuid()}";
    }

    private static bool IsSoftwareAgent(EArkAgent agent)
    {
        return agent.Type == EArkAgent.EType.Other && agent.OtherType == EArkConstants.SoftwareAgentOtherType;
    }

    private static bool IsSubmittingAgent(EArkAgent agent)
    {
        return agent.Role == EArkAgent.ERole.Creator && agent.Type != EArkAgent.EType.Other;
    }

    private static EArkFile WithId(EArkFile file)
    {
        return string.IsNullOrEmpty(file.Id) ? file with { Id = NewId() } : file;
    }

    private static EArkMetadataReference WithId(EArkMetadataReference reference)
    {
        return string.IsNullOrEmpty(reference.Id) ? reference with { Id = NewId() } : reference;
    }
}
