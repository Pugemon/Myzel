using Myzel.Lib.FileFormats.Bcsv.Converters;

namespace Myzel.Lib.FileFormats.Bcsv;

/// <summary>
/// A class holding parsing information about a BCSV header.
/// </summary>
public class BcsvHeaderInfo
{
    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvHeaderInfo"/> class.
    /// </summary>
    /// <param name="headerName">The name of the BCSV header in the file.</param>
    public BcsvHeaderInfo(string headerName) : this(headerName, headerName)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvHeaderInfo"/> class.
    /// </summary>
    /// <param name="headerName">The name of the BCSV header in the file.</param>
    /// <param name="dataType">The data type of the BCSV header.</param>
    public BcsvHeaderInfo(string headerName, BcsvDataType dataType) : this(headerName, headerName, dataType, null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvHeaderInfo"/> class.
    /// </summary>
    /// <param name="headerName">The name of the BCSV header in the file.</param>
    /// <param name="converter">The converter to use for the BCSV header values.</param>
    public BcsvHeaderInfo(string headerName, IBcsvConverter converter) : this(headerName, headerName, BcsvDataType.Default, converter)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvHeaderInfo"/> class.
    /// </summary>
    /// <param name="headerName">The name of the BCSV header in the file.</param>
    /// <param name="newHeaderName">The name of the BCSV header to use in the parsed result.</param>
    /// <param name="dataType">The data type of the BCSV header.</param>
    public BcsvHeaderInfo(string headerName, string newHeaderName, BcsvDataType dataType) : this(headerName, newHeaderName, dataType, null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvHeaderInfo"/> class.
    /// </summary>
    /// <param name="headerName">The name of the BCSV header in the file.</param>
    /// <param name="newHeaderName">The name of the BCSV header to use in the parsed result.</param>
    /// <param name="converter">The converter to use for the BCSV header values.</param>
    public BcsvHeaderInfo(string headerName, string newHeaderName, IBcsvConverter converter) : this(headerName, newHeaderName, BcsvDataType.Default, converter)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvHeaderInfo"/> class.
    /// </summary>
    /// <param name="headerName">The name of the BCSV header in the file.</param>
    /// <param name="dataType">The data type of the BCSV header.</param>
    /// <param name="converter">The converter to use for the BCSV header values.</param>
    public BcsvHeaderInfo(string headerName, BcsvDataType dataType, IBcsvConverter converter) : this(headerName, headerName, dataType, converter)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvHeaderInfo"/> class.
    /// </summary>
    /// <param name="headerName">The name of the BCSV header in the file.</param>
    /// <param name="newHeaderName">The name of the BCSV header to use in the parsed result.</param>
    /// <param name="dataType">The data type of the BCSV header.</param>
    /// <param name="converter">The converter to use for the BCSV header values.</param>
    public BcsvHeaderInfo(string headerName, string newHeaderName, BcsvDataType dataType = BcsvDataType.Default, IBcsvConverter? converter = null)
    {
        HeaderName = headerName;
        NewHeaderName = newHeaderName;
        DataType = dataType;
        Converter = converter;
    }
    #endregion

    #region public properties
    /// <summary>
    /// Gets the name of the BCSV header in the file.
    /// </summary>
    public string HeaderName { get; }

    /// <summary>
    /// Gets the name of the BCSV header to use in the parsed result.
    /// </summary>
    public string NewHeaderName { get; }

    /// <summary>
    /// Gets the data type of the BCSV header.
    /// </summary>
    public BcsvDataType DataType { get; }

    /// <summary>
    /// Gets the converter to use for the BCSV header values.
    /// </summary>
    public IBcsvConverter? Converter { get; }
    #endregion
}