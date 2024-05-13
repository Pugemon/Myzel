using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Myzel.Lib.Utils.Extensions
{
    /// <summary>
    /// Provides extension methods for the Regex class.
    /// </summary>
    public static class RegexExtensions
    {
        /// <summary>
        /// Determines whether the specified regular expression finds a match in the specified input string.
        /// </summary>
        /// <param name="regex">The regular expression object.</param>
        /// <param name="input">The input string to search for a match.</param>
        /// <param name="match">When this method returns, contains the match found in the input string, if any; otherwise, null.</param>
        /// <returns>True if a match is found; otherwise, false.</returns>
        public static bool IsMatch(this Regex regex, string input, [MaybeNullWhen(false)] out Match match)
        {
            match = regex.Match(input);
            return match.Success;
        }
    }
}