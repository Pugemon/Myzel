namespace Myzel.Lib.FileFormats;

/// <summary>
/// An extension class for <see cref="IFileCompiler{T}"/> types.
/// </summary>
public static class FileCompilerExtensions
{
    /// <summary>
    /// Compiles a file format to a file.
    /// </summary>
    /// <param name="compiler">The <see cref="IFileCompiler{T}"/> instance to use.</param>
    /// <param name="file">The file to compile.</param>
    /// <param name="filePath">The path of the file to compile to.</param>
    public static void Compile<T>(this IFileCompiler<T> compiler, T file, string filePath) where T : class
    {
        ArgumentNullException.ThrowIfNull(compiler);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(filePath);

        using FileStream stream = File.OpenRead(filePath);
        compiler.Compile(file, stream);
    }

    /// <summary>
    /// Compiles a file format to a byte array.
    /// </summary>
    /// <param name="compiler">The <see cref="IFileCompiler{T}"/> instance to use.</param>
    /// <param name="file">The file to compile.</param>
    /// <returns>The file compiled as byte array.</returns>
    public static byte[] Compile<T>(this IFileCompiler<T> compiler, T file) where T : class
    {
        ArgumentNullException.ThrowIfNull(compiler);
        ArgumentNullException.ThrowIfNull(file);

        using MemoryStream stream = new MemoryStream();
        compiler.Compile(file, stream);
        return stream.ToArray();
    }
}