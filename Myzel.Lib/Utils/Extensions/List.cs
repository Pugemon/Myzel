namespace Myzel.Lib.Utils.Extensions;

public static class ListExtensions
{
    /// <summary>
    /// Performs a binary search in a sorted list and returns the index of the element that is immediately after the specified value.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the list.</typeparam>
    /// <typeparam name="TSearch">The type of the value to search for.</typeparam>
    /// <param name="list">The sorted list.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">The comparison function that determines the order of elements.</param>
    /// <returns>The index of the element that is immediately after the specified value.</returns>
    public static int BinarySearchNext<TItem, TSearch>(this IList<TItem> list, TSearch value, Func<TSearch, TItem, TItem, int> comparer)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(comparer);

        int lower = 0;
        int upper = list.Count - 1;

        while (lower <= upper)
        {
            int middle = lower + ((upper - lower) / 2);
            int comparisonResult = comparer(value, list[middle], middle + 1 < list.Count ? list[middle + 1] : list[middle]);
            switch (comparisonResult)
            {
                case < 0:
                    upper = middle - 1;
                    break;
                case > 0:
                    lower = middle + 1;
                    break;
                default:
                    return middle;
            }
        }

        return ~lower;
    }

    /// <summary>
    /// Inserts an item into a sorted list using the specified comparison function.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the list.</typeparam>
    /// <param name="list">The sorted list.</param>
    /// <param name="newItem">The new item to insert.</param>
    /// <param name="comparer">The comparison function that determines the order of elements.</param>
    public static void InsertSorted<TItem>(this IList<TItem> list, TItem newItem, Func<TItem, TItem, int> comparer)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(comparer);

        int indexToInsert = list.TakeWhile(existingItem => comparer(existingItem, newItem) < 0).Count();
        list.Insert(indexToInsert, newItem);
    }
}