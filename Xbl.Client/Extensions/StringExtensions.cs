using Xbl.Xbox360.Extensions;

namespace Xbl.Client.Extensions;

public static class StringExtensions
{
    public static string ToHexId(this string titleId)
    {
        var id = uint.Parse(titleId);
        var bytes = BitConverter.GetBytes(id);
        bytes.SwapEndian(4);
        return bytes.ToHex();
    }

    public static int FromHexId(this string hexId)
    {
        var bytes = hexId.FromHex();
        return BitConverter.ToInt32(bytes, 0);
    }
}