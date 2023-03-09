using System;
using System.Security.Cryptography;
using Eplicta.Mets.Entities;

namespace Eplicta.Mets;

public static class HashExtensions
{
    public enum Style { Regular, Base64 }

    public static string ToHash(this byte[] item, MetsData.EChecksumType method = MetsData.EChecksumType.MD5, Style style = Style.Regular)
    {
        byte[] hash;

        switch (method)
        {
            case MetsData.EChecksumType.MD5:
            {
                using var m = MD5.Create();
                hash = m.ComputeHash(item);
                break;
            }
            case MetsData.EChecksumType.SHA_1:
            {
                using var m = SHA1.Create();
                hash = m.ComputeHash(item);
                break;
            }
            case MetsData.EChecksumType.SHA_256:
            {
                using var m = SHA256.Create();
                hash = m.ComputeHash(item);
                break;
            }
            case MetsData.EChecksumType.SHA_384:
            {
                using var m = SHA384.Create();
                hash = m.ComputeHash(item);
                break;
            }
            case MetsData.EChecksumType.SHA_512:
            {
                using var m = SHA512.Create();
                hash = m.ComputeHash(item);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException($"Unknown method '{method}'.");
        }

        return Format(style, hash);
    }

    private static string Format(Style style, byte[] hash)
    {
        switch (style)
        {
            case Style.Regular:
                return BitConverter.ToString(hash).Replace("-", "");
            case Style.Base64:
                return Convert.ToBase64String(hash);
            default:
                throw new ArgumentOutOfRangeException($"Unknown style '{style}'.");
        }
    }
}