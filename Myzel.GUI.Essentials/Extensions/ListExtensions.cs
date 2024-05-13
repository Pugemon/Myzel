using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;


namespace Myzel.GUI.Essentials.Extensions;

/// <summary>
/// Extensions for working with IList collections.
/// </summary>
public static class ListExtensions
{
    
    
    /// <summary>
    ///     Merges two lists together without replacing original values
    ///     All new elements will added to the tail. All elements not existing in collection2 will be removed from collection1!
    /// </summary>
    public static void Merge(this Collection<string> collection1, Collection<string> collection2)
    {
        List<string> diff = [];
        
        //Add all new elements to diff list
        diff.AddRange(collection2.Where(t => !collection1.Contains(t)));
        //remove all deleted elements
        for (int i = 0; i < collection1.Count; i++)
            if (!collection2.Contains(collection1[i]))
            {
                collection1.RemoveAt(i);
                i--;
            }

        //Add new elements to end of old list
        collection1.AddRange(diff);
    }

    public static int Remove<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
    {
        List<T> itemsToRemove = coll.Where(condition).ToList();

        foreach (T itemToRemove in itemsToRemove) coll.Remove(itemToRemove);

        return itemsToRemove.Count;
    }
}