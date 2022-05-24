using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eplicta.Mets.Entities;
using MimeTypes;

namespace Eplicta.Mets;

public class Builder
{
    private ModsData.AgentData _agentData = new();
    private ModsData.CompanyData _companyData = new();
    private ModsData.SoftwareData _softwareData = new ();
    private readonly List<ModsData.AltRecord> _altRecords = new();
    private ModsData.ModsSectionData _modsSectionData = new()
    {
        Url = new Uri("https://some.url"),
        ModsTitle = "Unknown",
        ModsTitleInfo = "Unknown"
    };
    private readonly List<ModsData.FileData> _fileDatas = new();

    public ModsData Build()
    {
        if (!_fileDatas.Any()) throw new InvalidOperationException("At least one file has to be added.");
        if (_altRecords.Count < 3) throw new InvalidOperationException("At least thre altRecord has to be added.");

        return new ModsData
        {
            Agent = _agentData,
            Company = _companyData,
            Software = _softwareData,
            Mods = _modsSectionData,
            Files = _fileDatas.ToArray(),
            AltRecords = _altRecords.ToArray()
        };
    }

    public Builder AddFile(FileSource fileSource)
    {
        var fileName = fileSource.FileName;
        var  id = fileSource.Id;
        var data = fileSource.Data;
        var created = fileSource.CreationTime;

        if (!string.IsNullOrEmpty(fileSource.FilePath))
        {
            var fileInfo = new FileInfo(fileSource.FilePath);
            data = File.ReadAllBytes(fileSource.FilePath);
            fileName ??= fileInfo.Name;
            created ??= fileInfo.CreationTime;
        }

        id ??= data.ToHash();

        var mimeType = GetMimeType(fileName);

        var fileData = new ModsData.FileData
        {
            Id = id,
            Use = fileSource.Use, //TODO: "Acrobat PDF/X - Portable Document Format - Exchange 1:1999;PRONOM:fmt/144"
            MimeType = mimeType,
            Data = data ?? throw new NullReferenceException("No data provided or found."),
            Size = data.Length,
            Created = created ?? DateTime.MinValue,
            LocType = ModsData.ELocType.Url,
            FileName = fileName
        };

        if (fileSource.ChecksumType != null)
        {
            var checksum = data.ToHash(fileSource.ChecksumType.Value, HashExtensions.Style.Base64);
            fileData = fileData with
            {
                Checksum = checksum,
                ChecksumType = fileSource.ChecksumType.Value,
            };
        }

        _fileDatas.Add(fileData);

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
}