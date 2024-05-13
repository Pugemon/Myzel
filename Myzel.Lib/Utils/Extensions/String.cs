namespace Myzel.Lib.Utils.Extensions;

public static class StringExtensions
    {
        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        public static bool EqualPaths(this string input, string otherPath)
        {
            if(string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(otherPath)) return input.Trim() == otherPath.Trim();
            return Path.GetFullPath(input).TrimEnd('\\').Equals(Path.GetFullPath(otherPath).TrimEnd('\\'),
                StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool ContainsSubPath(this string pathToFile, string subPath)
        {
            pathToFile = Path.GetDirectoryName(pathToFile) + "\\";
            string searchPath = Path.GetDirectoryName(subPath) + "\\";

            bool containsIt = pathToFile.IndexOf(searchPath, StringComparison.OrdinalIgnoreCase) > -1;

            return containsIt;
        }

        public static string ToPlatformPath(this string input)
        {
            return input
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
        }
        
        public static string ToUnixPath(this string input)
        {
            return input.Replace('\\', '/');
        }
        
        public static string NormalizePath(this string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }
        
        public static bool IsValidFileName(this string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            char[] chars = Path.GetInvalidFileNameChars();
            return chars.All(character => !name.Contains(character));
        }

        
        public static string CheckNameFile(this string fullPath)
        {
            string folderPath = Path.GetDirectoryName(fullPath) ?? "";
            string fileName = Path.GetFileNameWithoutExtension(fullPath) ?? "unknown";
            string extension = Path.GetExtension(fullPath) ?? "";
            for (int i = 1; File.Exists(fullPath); i++)
            {
                fullPath = Path.Combine(folderPath, fileName + i + extension);
            }
            return fullPath;
        }
        
        public static string CheckNameDirectory(this string fullPath)
        {
            string folderPath = Path.GetDirectoryName(fullPath) ?? "";
            string fileName = Path.GetFileNameWithoutExtension(fullPath) ?? "unknown";
            for (int i = 1; Directory.Exists(fullPath); i++)
            {
                fullPath = Path.Combine(folderPath, fileName + i);
            }
            return fullPath;
        }
    }