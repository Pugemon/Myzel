namespace Myzel.Lib.Utils.Extensions
{
    /// <summary>
    /// Provides extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Attempts to add an element to the collection if the specified predicate evaluates to true for all existing elements in the collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to add the element to.</param>
        /// <param name="element">The element to add to the collection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>True if the element was successfully added to the collection; otherwise, false.</returns>
        public static bool TryAdd<T>(this ICollection<T> source, T element, Func<T, bool> predicate)
        {
            // Check if the predicate evaluates to true for all existing elements in the collection
            if (source.Any(item => !predicate(item)))
                return false;

            // Add the element to the collection
            source.Add(element);
            return true;
        }
    }
}