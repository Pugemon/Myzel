using System.Text;

namespace Myzel.Lib.Utils.Extensions;

internal static class EncodingExtensions
{
    public static int GetMinByteCount(this Encoding encoding)
    {
        return encoding.GetByteCount("\0");
    }
}