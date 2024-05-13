using System.Drawing;

namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// A class representing a color parameter in an AAMP file.
/// </summary>
public class ColorParameter : Parameter
{
    #region public properties
    /// <inheritdoc/>
    public override byte Type => ParameterTypes.Color;

    /// <summary>
    /// Gets or sets the red channel value of the color.
    /// </summary>
    public float Red { get; init; }

    /// <summary>
    /// Gets or sets the green channel value of the color.
    /// </summary>
    public float Green { get; init; }

    /// <summary>
    /// Gets or sets the blue channel value of the color.
    /// </summary>
    public float Blue { get; init; }

    /// <summary>
    /// Gets or sets the alpha channel value of the color.
    /// </summary>
    public float Alpha { get; init; }
    #endregion

    #region public methods
    /// <summary>
    /// Gets a <see cref="Color"/> instance for the color.
    /// </summary>
    /// <returns>A new <see cref="Color"/> instance.</returns>
    public Color ToColor() => Color.FromArgb(GetColorByte(Alpha), GetColorByte(Red), GetColorByte(Green), GetColorByte(Blue));

    /// <summary>
    /// Gets the hex string representation of the color.
    /// </summary>
    /// <param name="includeAlpha">Whether to include the alpha channel value.</param>
    /// <returns>A hex-color string.</returns>
    public string ToHexString(bool includeAlpha = false)
    {
        string hex = $"#{GetColorByte(Red):X2}{GetColorByte(Green):X2}{GetColorByte(Blue):X2}";
        if (includeAlpha) hex += $"{GetColorByte(Alpha):X2}";
        return hex;
    }
    #endregion

    #region private methods
    private static int GetColorByte(float value) => (int) (value * 255);
    #endregion
}