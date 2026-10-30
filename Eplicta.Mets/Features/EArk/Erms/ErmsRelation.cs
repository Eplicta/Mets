using System.Xml.Serialization;

namespace Eplicta.Mets.Features.EArk.Erms;

/// <summary>
/// A relation to another entity, named by its identifier, such as a case that refers to another case.
/// </summary>
public record ErmsRelation
{
    public enum ERelationType
    {
        [XmlEnum("replaces")] Replaces,
        [XmlEnum("is_replaced_with")] IsReplacedWith,
        [XmlEnum("reference")] Reference,
        [XmlEnum("referenced_by")] ReferencedBy,
        [XmlEnum("demands")] Demands,
        [XmlEnum("needed_by")] NeededBy,
        [XmlEnum("contains")] Contains,
        [XmlEnum("part_of")] PartOf,
        [XmlEnum("other_format_version")] OtherFormatVersion,
        [XmlEnum("another_format_version_of")] AnotherFormatVersionOf,
        [XmlEnum("has_version")] HasVersion,
        [XmlEnum("is_version_of")] IsVersionOf,
        [XmlEnum("is_redacted_version_of")] IsRedactedVersionOf,
        [XmlEnum("has_redacted_version")] HasRedactedVersion,
        [XmlEnum("rendition_version_of")] RenditionVersionOf,
        [XmlEnum("has_rendition_version")] HasRenditionVersion,
        [XmlEnum("is_child_of")] IsChildOf,
        [XmlEnum("is_parent_of")] IsParentOf,
        [XmlEnum("moved")] Moved,
        [XmlEnum("moved_from")] MovedFrom,
        [XmlEnum("deleted")] Deleted,
        [XmlEnum("own_relation_definition")] OwnRelationDefinition
    }

    public ERelationType RelationType { get; init; }
    public string OtherRelationType { get; init; }
    public string Value { get; init; }
}
