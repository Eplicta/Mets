using System.Collections.Generic;
using System.Xml;

namespace Eplicta.Mets;

public interface IMetsValidatorService
{
    IEnumerable<ValidatorResult> Validate(XmlDocument document);
}