using System;
using System.Collections.Generic;
using System.Linq;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

public class Builder
{
    private readonly List<MetsData.AltRecord> _altRecords = new();
    private readonly List<MetsData.FileData> _fileDatas = new();
    private readonly List<StreamFileSource> _streamSources = new();
    private readonly MetsData.EMetsAttributeName[] _requiredMetsAttributes = { MetsData.EMetsAttributeName.ObjId };
    private MetsData.AgentData _agentData = new();
    private MetsData.CompanyData _companyData = new();
    private MetsData.SoftwareData _softwareData = new();
    private MetsData.ModsSectionData _modsSectionData;
    private MetsData.MetsHdrData _metsHdrData = new();
    private MetsData.MetsAttribute[] _attributes = [];
    private string _metsProfile = "http://www.kb.se/namespace/mets/fgs/eARD_Paket_FGS-PUBL.xml";

    public MetsData Build()
    {
        var missingAttributes = _requiredMetsAttributes.Where(x => _attributes.All(y => y.Name != x)).ToArray();
        var multiple = missingAttributes.Length > 1;

        if (missingAttributes.Length > 0) throw new InvalidOperationException($"The required attribute{(multiple ? "s" : string.Empty)} {string.Join(",", missingAttributes)} {(multiple ? "are" : "is")} missing.");

        return new MetsData
        {
            Agent = _agentData,
            Company = _companyData,
            Software = _softwareData,
            Mods = _modsSectionData,
            Files = _fileDatas?.ToArray(),
            Sources = _streamSources.ToArray(),
            AltRecords = _altRecords.ToArray(),
            MetsHdr = _metsHdrData,
            Attributes = _attributes,
            MetsProfile = _metsProfile
        };
    }

    public Builder AddFile(SourceBase source)
    {
        switch (source)
        {
            case StreamFileSource streamSource:
            {
                source.Name = CheckForDuplicateFileNames(source.Name);
                _streamSources.Add(streamSource);
                break;
            }
            case DataFileSource fileSource:
            {
                var fileName = fileSource.Name;
                var id = fileSource.Id;
                var data = fileSource.Data;
                var created = fileSource.CreationTime;

                if (data == null) throw new NullReferenceException("No data provided or found.");

                var fileData = new MetsData.FileData
                {
                    Id = id ?? $"ID{data.ToHash()}",
                    Use = fileSource.Use, //TODO: "Acrobat PDF/X - Portable Document Format - Exchange 1:1999;PRONOM:fmt/144"
                    MimeType = fileSource.MimeType ?? FileExtensions.GetMimeType(fileName),
                    Data = data,
                    Size = fileSource.Size ?? data.Length,
                    Created = created ?? DateTime.MinValue,
                    LocType = MetsData.ELocType.Url,
                    FileName = CheckForDuplicateFileNames(fileName)
                };

                if (fileSource.ChecksumType != null)
                {
                    fileData = fileData with
                    {
                        ChecksumType = fileSource.ChecksumType.Value,
                        Checksum = fileSource.Checksum ?? data.ToHash(fileSource.ChecksumType.Value, HashExtensions.Style.Base64)
                    };
                }

                _fileDatas.Add(fileData);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }

        return this;
    }
    private string CheckForDuplicateFileNames(string fileName)
    {
        var fileNames = _fileDatas.Select(x => x.FileName).ToArray();

        if (fileNames.Length == 0 || fileNames.All(x => x != fileName)) return fileName;

        string temp;

        var i = 1;
        do
        {
            var name = fileName.Split('.')[0];
            var ext = fileName.Split('.')[1];

            name = $"{name}({i})";

            temp = $"{name}.{ext}";

            i++;

        } while (fileNames.Any(x => x == temp));

        return temp;
    }

    public Builder AddFiles(IEnumerable<DataFileSource> fileSources)
    {
        foreach (var fileSource in fileSources)
        {
            AddFile(fileSource);
        }

        return this;
    }

    public Builder SetAgent(MetsData.AgentData agentData)
    {
        _agentData = agentData;
        return this;
    }

    public Builder SetCompany(MetsData.CompanyData companyData)
    {
        _companyData = companyData;
        return this;
    }

    public Builder AddAltRecord(MetsData.AltRecord altRecord)
    {
        _altRecords.Add(altRecord);
        return this;
    }

    public Builder SetModsSection(MetsData.ModsSectionData modsSectionData)
    {
        _modsSectionData = modsSectionData;
        return this;
    }

    public Builder SetSoftware(MetsData.SoftwareData softwareData)
    {
        _softwareData = softwareData;
        return this;
    }

    public Builder SetMetsHdr(MetsData.MetsHdrData metsHdrData)
    {
        _metsHdrData = metsHdrData;
        return this;
    }

    public Builder SetMetsHdrAttributes(MetsData.MetsHdrAttribute[] attributes)
    {
        _metsHdrData.Attributes = attributes;
        return this;
    }

    public Builder AddMetsHdrAttributes(MetsData.MetsHdrAttribute[] attributes)
    {
        _metsHdrData.Attributes = _metsHdrData.Attributes.Concat(attributes).ToArray();
        return this;
    }

    public Builder SetMetsAttributes(MetsData.MetsAttribute[] attributes)
    {
        _attributes = attributes;
        return this;
    }

    public Builder AddMetsAttributes(MetsData.MetsAttribute[] attribute)
    {
        _attributes = _attributes.Concat(attribute).ToArray();
        return this;
    }

    public Builder SetMetsProfile(string profile)
    {
        _metsProfile = profile;
        return this;
    }

    public Builder AddModsNote(MetsData.ModsNote note)
    {
        var notesArray = _modsSectionData.Notes?.Concat([note]).ToArray() ?? [note];

        _modsSectionData.Notes = notesArray;
        return this;
    }
}