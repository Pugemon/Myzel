namespace Myzel.Lib.Utils.Extensions;

internal static class StackExtensions
{
    public static bool TryPeek<T>(this Stack<T> stack, out T? value)
    {
        switch (stack.Count)
        {
            case > 0:
                value = stack.Peek();
                return true;
            default:
                value = default;
                return false;
        }

    }
}