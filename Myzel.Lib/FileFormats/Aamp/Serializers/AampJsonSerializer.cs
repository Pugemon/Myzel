using Myzel.Lib.FileFormats.Aamp.Parameters;
using Newtonsoft.Json;

namespace Myzel.Lib.FileFormats.Aamp.Serializers;

/// <summary>
/// A class for serializing <see cref="AampFile"/> objects to JSON.
/// </summary>
public class AampJsonSerializer : IFileSerializer<AampFile>
{
    #region public properties
    /// <summary>
    /// Gets or sets number of indentation characters that should be used.
    /// '<c>0</c>' disables indentation.
    /// The default value is <c>2</c>.
    /// </summary>
    public int Indentation { get; set; } = 2;

    /// <summary>
    /// Gets or sets the indentation character that should be used.
    /// The default value is '<c> </c>'.
    /// </summary>
    public char IndentChar { get; set; } = ' ';
    #endregion

    #region IFileSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, AampFile file)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        using JsonTextWriter jsonWriter = new(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("version");
        jsonWriter.WriteValue(file.Version);
        WriteList(jsonWriter, file.Root);
        jsonWriter.WriteEndObject();
    }
    #endregion

    #region private methods
    private static void WriteList(JsonWriter writer, ParameterList list)
    {
        writer.WritePropertyName(list.Name);
        writer.WriteStartObject();
        foreach (ParameterList subList in list.Lists)
        {
            WriteList(writer, subList);
        }
        foreach (ParameterObject obj in list.Objects)
        {
            WriteObject(writer, obj);
        }
        writer.WriteEndObject();
    }

    private static void WriteObject(JsonWriter writer, ParameterObject obj)
    {
        writer.WritePropertyName(obj.Name);
        writer.WriteStartObject();
        foreach (Parameter parameter in obj.Parameters)
        {
            WriteParameter(writer, parameter);
        }
        writer.WriteEndObject();
    }

    private static void WriteParameter(JsonWriter writer, Parameter parameter)
    {
        Formatting defaultFormat = writer.Formatting;
        writer.WritePropertyName(parameter.Name);
        switch (parameter)
        {
            case ColorParameter color:
                writer.WriteStartArray();
                writer.WriteValue(color.Red);
                writer.WriteValue(color.Green);
                writer.WriteValue(color.Blue);
                writer.WriteValue(color.Alpha);
                writer.WriteEndArray();
                break;
            case CurveParameter curve:
                writer.WriteStartArray();
                foreach (CurveValue value in curve.Curves)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("intValues");
                    writer.WriteStartArray();
                    foreach (uint intVal in value.IntValues) writer.WriteValue(intVal);
                    writer.WriteEndArray();
                    writer.WritePropertyName("floatValues");
                    writer.WriteStartArray();
                    foreach (float floatVal in value.FloatValues) writer.WriteValue(floatVal);
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                break;
            case ValueParameter value:
                if (value.Value is Array array)
                {
                    writer.Formatting = Formatting.None;
                    writer.WriteStartArray();
                    foreach (object? item in array) writer.WriteValue(item);
                    writer.WriteEndArray();
                    writer.Formatting = defaultFormat;
                }
                else writer.WriteValue(value.Value);
                break;
        }
    }
    #endregion
}