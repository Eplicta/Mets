using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eplicta.Mets.Entities;
using MimeTypes;

namespace Eplicta.Mets;

public class Builder
{
    private readonly List<ModsData.AltRecord> _altRecords = new();
    private readonly List<ModsData.FileData> _fileDatas = new();
    private readonly ModsData.EMetsAttributeName[] _requiredMetsAttributes = { ModsData.EMetsAttributeName.ObjId };
    private ModsData.AgentData _agentData = new();
    private ModsData.CompanyData _companyData = new();
    private ModsData.SoftwareData _softwareData = new();
    private ModsData.ModsSectionData _modsSectionData = new()
    {
        Url = new Uri("https://some.url"),
        ModsTitle = "Unknown",
        ModsTitleInfo = "Unknown"
    };
    private ModsData.MetsHdrData _metsHdrData = new();
    private ModsData.MetsAttribute[] _attributes = Array.Empty<ModsData.MetsAttribute>();

    public ModsData Build()
    {
        if (_altRecords.Count < 3) throw new InvalidOperationException("At least three altRecords has to be added.");

        var missingAttributes = _requiredMetsAttributes.Where(x => !_attributes.Any(y => y.Name == x)).ToArray();
        var multiple = missingAttributes.Length > 1;

        if (missingAttributes.Length > 0) throw new InvalidOperationException($"The required attribute{(multiple ? "s" : string.Empty)} {string.Join(",", missingAttributes)} {(multiple ? "are" : "is")} missing.");

        return new ModsData
        {
            Agent = _agentData,
            Company = _companyData,
            Software = _softwareData,
            Mods = _modsSectionData,
            Files = _fileDatas?.ToArray(),
            AltRecords = _altRecords.ToArray(),
            MetsHdr = _metsHdrData,
            Attributes = _attributes
        };
    }

    public Builder AddFile(FileSource fileSource)
    {
        var fileName = fileSource.FileName;
        var id = fileSource.Id;
        var data = fileSource.Data;
        var created = fileSource.CreationTime;

        if (!string.IsNullOrEmpty(fileSource.FilePath))
        {
            var fileInfo = new FileInfo(fileSource.FilePath);
            data = File.ReadAllBytes(fileSource.FilePath);
            fileName ??= fileInfo.Name;
            created ??= fileInfo.CreationTime;
        }

        if (data == null) throw new NullReferenceException("No data provided or found.");

        var fileData = new ModsData.FileData
        {
            Id = id ?? $"ID{new Guid(data.ToHash())}",
            Use = fileSource.Use, //TODO: "Acrobat PDF/X - Portable Document Format - Exchange 1:1999;PRONOM:fmt/144"
            MimeType = fileSource.MimeType ?? GetMimeType(fileName),
            Data = data,
            Size = fileSource.Size ?? data.Length,
            Created = created ?? DateTime.MinValue,
            LocType = ModsData.ELocType.Url,
            FileName = CheckForDuplicateFileNames(fileName)
            //Ns2Href = fileSource.Ns2Href
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

        return this;
    }

    private string CheckForDuplicateFileNames(string fileName)
    {
        var fileNames = _fileDatas.Select(x => x.FileName).ToArray();

        if (fileNames.Length == 0 || !fileNames.Any(x => x == fileName)) return fileName;

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

    public Builder AddFiles(IEnumerable<FileSource> fileSources)
    {
        foreach (var fileSource in fileSources)
        {
            AddFile(fileSource);
        }

        return this;
    }

    private string GetMimeType(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        var type = fileName.Split('.').Last();
        var mimeType = MimeTypeMap.GetMimeType(type);
        return mimeType;
    }

    public Builder SetAgent(ModsData.AgentData agentData)
    {
        _agentData = agentData;
        return this;
    }

    public Builder SetCompany(ModsData.CompanyData companyData)
    {
        _companyData = companyData;
        return this;
    }

    public Builder AddAltRecord(ModsData.AltRecord altRecord)
    {
        _altRecords.Add(altRecord);
        return this;
    }

    public Builder SetModsSection(ModsData.ModsSectionData modsSectionData)
    {
        _modsSectionData = modsSectionData;
        return this;
    }

    public Builder SetSoftware(ModsData.SoftwareData softwareData)
    {
        _softwareData = softwareData;
        return this;
    }

    public Builder SetMetsHdr(ModsData.MetsHdrData metsHdrData)
    {
        _metsHdrData = metsHdrData;
        return this;
    }

    public Builder SetMetsHdrAttributes(ModsData.MetsHdrAttribute[] attributes)
    {
        _metsHdrData.Attributes = attributes;
        return this;
    }

    public Builder AddMetsHdrAttributes(ModsData.MetsHdrAttribute[] attributes)
    {
        _metsHdrData.Attributes = _metsHdrData.Attributes.Concat(attributes).ToArray();
        return this;
    }

    public Builder SetMetsAttributes(ModsData.MetsAttribute[] attributes)
    {
        _attributes = attributes;
        return this;
    }

    public Builder AddMetsAttributes(ModsData.MetsAttribute[] attribute)
    {
        _attributes = _attributes.Concat(attribute).ToArray();
        return this;
    }
}