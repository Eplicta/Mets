using System;
using System.Linq;
using System.Xml;
using Eplicta.Mets.Features.EArk.Premis;
using FluentAssertions;
using Xunit;

namespace Eplicta.Mets.Tests.Features.EArk.Premis;

public class PremisRendererTests
{
    private static readonly DateTime Created = new(2026, 3, 14, 9, 30, 0, DateTimeKind.Utc);

    private static PremisFile File()
    {
        return new PremisFile
        {
            Identifier = PremisIdentifier.Local("file-1"),
            Fixities = [new PremisFixity { Digest = "6021a1b2c3", Originator = "FortDocs" }],
            Size = 74171,
            FormatName = "application/pdf",
            FormatVersion = "1.7",
            FormatRegistryKey = "fmt/276",
            OriginalName = "Anmälan.pdf",
            ContentLocation = "representations/rep_1/data/2026-0042-1_1_Anmalan.pdf",
            LinkingEventIdentifiers = [PremisIdentifier.Local("event-1")]
        };
    }

    private static PremisDocument Full()
    {
        return new PremisDocument
        {
            Files = [File(), new PremisFile { Identifier = PremisIdentifier.Local("file-2"), FormatName = "text/plain" }],
            Events =
            [
                new PremisEvent
                {
                    Identifier = PremisIdentifier.Local("event-1"),
                    EventType = "fixity check",
                    DateTime = Created,
                    Detail = "Every file's bytes were read and compared with the SHA-256 stored when it was uploaded.",
                    Outcome = "success",
                    OutcomeDetailNote = "2 of 2 files matched.",
                    LinkingAgents = [new PremisLinkingAgent { Identifier = PremisIdentifier.Local("agent-1"), Role = "executing program" }],
                    LinkingObjects = [PremisIdentifier.Local("file-1"), PremisIdentifier.Local("file-2")]
                }
            ],
            Agents = [new PremisAgent { Identifier = PremisIdentifier.Local("agent-1"), Name = "FortDocs", Type = "software", Version = "1.4.140", Notes = ["Eplicta"] }]
        };
    }

    private static string Errors(XmlDocument document)
    {
        return string.Join(Environment.NewLine, new PremisValidator().Validate(document).Select(x => $"{x.XmlSeverityType}: {x.Message}"));
    }

    private static XmlNamespaceManager Namespaces(XmlDocument document)
    {
        var manager = new XmlNamespaceManager(document.NameTable);
        manager.AddNamespace("p", PremisConstants.Namespace);
        manager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        return manager;
    }

    [Fact]
    public void A_single_file_validates_against_the_premis_schema()
    {
        var document = new PremisDocument { Files = [new PremisFile { Identifier = PremisIdentifier.Local("file-1"), FormatName = "application/pdf" }] };

        Errors(new PremisRenderer(document).Render()).Should().BeEmpty();
    }

    [Fact]
    public void Files_events_and_agents_with_everything_we_model_validate_against_the_premis_schema()
    {
        Errors(new PremisRenderer(Full()).Render()).Should().BeEmpty();
    }

    [Fact]
    public void A_file_identified_only_by_its_registry_key_validates()
    {
        var document = new PremisDocument { Files = [new PremisFile { Identifier = PremisIdentifier.Local("file-1"), FormatRegistryKey = "fmt/276" }] };

        Errors(new PremisRenderer(document).Render()).Should().BeEmpty();
    }

    [Fact]
    public void The_document_is_premis_3_and_points_at_the_schema_the_package_carries()
    {
        var root = new PremisRenderer(Full()).Render().DocumentElement!;

        root.LocalName.Should().Be("premis");
        root.NamespaceURI.Should().Be(PremisConstants.Namespace);
        root.GetAttribute("version").Should().Be("3.0");
        root.GetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance").Should().Be("http://www.loc.gov/premis/v3 ../../schemas/premis-v3-0.xsd");
    }

    [Fact]
    public void Every_object_is_typed_as_a_file()
    {
        var document = new PremisRenderer(Full()).Render();

        document.SelectNodes("//p:object", Namespaces(document))!.Cast<XmlElement>().Should().OnlyContain(x => x.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance") == "premis:file");
    }

    [Fact]
    public void Objects_come_before_events_and_events_before_agents()
    {
        var root = new PremisRenderer(Full()).Render().DocumentElement!;

        root.ChildNodes.OfType<XmlElement>().Select(x => x.LocalName).Should().Equal("object", "object", "event", "agent");
    }

    [Fact]
    public void A_document_without_a_file_is_reported_since_premis_requires_one()
    {
        Errors(new PremisRenderer(new PremisDocument()).Render()).Should().NotBeEmpty();
    }

    [Fact]
    public void A_file_without_a_format_is_reported()
    {
        var document = new PremisDocument { Files = [new PremisFile { Identifier = PremisIdentifier.Local("file-1") }] };

        Errors(new PremisRenderer(document).Render()).Should().NotBeEmpty();
    }

    [Fact]
    public void The_schema_ships_inside_the_library()
    {
        PremisRenderer.Schema().Should().Contain("targetNamespace=\"http://www.loc.gov/premis/v3\"");
    }
}
