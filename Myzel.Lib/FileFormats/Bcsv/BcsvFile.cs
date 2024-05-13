using System.Collections;

namespace Myzel.Lib.FileFormats.Bcsv;

/// <summary>
/// A class holding information about a BCSV file.
/// </summary>
public class BcsvFile : IEnumerable<object?[]>
{
    #region private members
    private readonly object?[][] _cells;
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvFile"/> class.
    /// </summary>
    /// <param name="headerInfo">A collection of headers.</param>
    /// <param name="cells">A collection of data cells.</param>
    internal BcsvFile(BcsvHeaderInfo[] headerInfo, object?[][] cells)
    {
        HeaderInfo = headerInfo;
        _cells = cells;
    }
    #endregion

    #region public properties
    /// <summary>
    /// Gets the list of header names in the BCSV file.
    /// </summary>
    public BcsvHeaderInfo[] HeaderInfo { get; }

    /// <summary>
    /// Gets the value of a specific cell.
    /// </summary>
    /// <param name="row">The row of the cell.</param>
    /// <param name="header">The name of the header.</param>
    /// <returns>The value of the specific cell.</returns>
    public object? this[int row, string header]
    {
        get => _cells[row][GetHeaderIndex(header)];
        set => _cells[row][GetHeaderIndex(header)] = value;
    }

    /// <summary>
    /// Gets the value of a specific cell.
    /// </summary>
    /// <param name="row">The row of the cell.</param>
    /// <param name="col">The column of the cell.</param>
    /// <returns>The value of the specific cell.</returns>
    public object? this[int row, int col]
    {
        get => _cells[row][col];
        set => _cells[row][col] = value;
    }

    /// <summary>
    /// Gets the number of rows in the BCSV file.
    /// </summary>
    public int Rows => _cells.Length;

    /// <summary>
    /// Gets the number of columns in the BCSV file.
    /// </summary>
    public int Columns => _cells[0].Length;
    #endregion

    #region private methods
    private int GetHeaderIndex(string headerName)
    {
        for (var i = 0; i < HeaderInfo.Length; ++i)
        {
            if (headerName.Equals(HeaderInfo[i].NewHeaderName, StringComparison.OrdinalIgnoreCase)) return i;
        }

        for (var i = 0; i < HeaderInfo.Length; ++i)
        {
            if (headerName.Equals(HeaderInfo[i].HeaderName, StringComparison.OrdinalIgnoreCase)) return i;
        }

        throw new IndexOutOfRangeException($"Header with name \"{headerName}\" does not exist in this BCSV file.");
    }
    #endregion

    #region IEnumerable interface
    /// <inheritdoc />
    public IEnumerator<object?[]> GetEnumerator()
    {
        for (var row = 0; row < Rows; ++row) yield return _cells[row];
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}