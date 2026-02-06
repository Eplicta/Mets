using System.Collections.Generic;
using System.Xml;

namespace Eplicta.Mets;

public interface IValidatorService
{
    IEnumerable<ValidatorResult> Validate(XmlDocument document);
}