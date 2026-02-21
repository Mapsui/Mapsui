using System;
using System.Security.Cryptography;
using System.Text;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

internal static class Utils
{
    public static double ConvertRange(double oldValue, double oldMin, double oldMax, double newMin, double newMax, bool clamp = false)
    {
        double num = oldMax - oldMin;
        double num2;
        if (num == 0.0)
        {
            num2 = newMin;
        }
        else
        {
            double num3 = newMax - newMin;
            num2 = (oldValue - oldMin) * num3 / num + newMin;
        }
        if (clamp)
        {
            num2 = Math.Min(Math.Max(num2, newMin), newMax);
        }
        return num2;
    }

    public static string Sha256(string randomString)
    {
        using SHA256 sHA = SHA256.Create();
        StringBuilder stringBuilder = new StringBuilder();
        byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(randomString));
        foreach (byte b in array)
        {
            stringBuilder.Append(b.ToString("x2"));
        }
        return stringBuilder.ToString();
    }
}
