using System;
using Tharga.Toolkit.Console.Entities;

namespace Eplicta.Mets.Console.Commands.Mets;

public static class ValidatorResultExtensions
{
    public static OutputLevel ToLevel(this ValidatorResult item)
    {
        OutputLevel level;
        switch (item.Severity)
        {
            case SeverityType.Error:
                level = OutputLevel.Error;
                break;
            case SeverityType.Warning:
                level = OutputLevel.Warning;
                break;
            case SeverityType.Information:
                level = OutputLevel.Information;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return level;
    }
}