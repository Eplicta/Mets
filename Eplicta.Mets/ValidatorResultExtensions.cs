namespace Eplicta.Mets;

public static class ValidatorResultExtensions
{
    public static string ToMessage(this ValidatorResult item)
    {
        return item.Information ?? $"{item.XmlReslut.Message} (Line: {item.XmlReslut.XmlSchemaException.LineNumber}, Position: {item.XmlReslut.XmlSchemaException.LinePosition})";
    }
}