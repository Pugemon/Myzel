using Myzel.Lib.FileFormats.Aamp.Parameters;
using Myzel.Lib.Utils.YamlTextWriter;

namespace Myzel.Lib.FileFormats.Aamp.Serializers;

/// <summary>
/// A class for serializing <see cref="AampFile"/> objects to JSON.
/// </summary>
public class AampYamlSerializer : IFileSerializer<AampFile>
{
    #region IFileSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, AampFile file)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        using YamlTextWriter yamlWriter = new(writer);

        yamlWriter.WriteStartDocument();
        yamlWriter.WritePropertyName("version");
        yamlWriter.WriteValue(file.Version);
        WriteList(yamlWriter, file.Root);
    }
    #endregion

    #region private methods
    private static void WriteList(YamlTextWriter writer, ParameterList list)
    {
        writer.WritePropertyName(list.Name);
        writer.WriteStartDictionary();
        foreach (ParameterList subList in list.Lists)
        {
            WriteList(writer, subList);
        }
        foreach (ParameterObject obj in list.Objects)
        {
            WriteObject(writer, obj);
        }
        writer.WriteEndDictionary();
    }

    private static void WriteObject(YamlTextWriter writer, ParameterObject obj)
    {
        writer.WritePropertyName(obj.Name);
        writer.WriteStartDictionary();
        foreach (Parameter parameter in obj.Parameters)
        {
            WriteParameter(writer, parameter);
        }
        writer.WriteEndDictionary();
    }

    private static void WriteParameter(YamlTextWriter writer, Parameter parameter)
    {
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
                    writer.WriteStartDictionary();
                    writer.WritePropertyName("intValues");
                    writer.WriteStartArray();
                    foreach (uint intVal in value.IntValues) writer.WriteValue(intVal);
                    writer.WriteEndArray();
                    writer.WritePropertyName("floatValues");
                    writer.WriteStartArray();
                    foreach (float floatVal in value.FloatValues) writer.WriteValue(floatVal);
                    writer.WriteEndArray();
                    writer.WriteEndDictionary();
                }
                writer.WriteEndArray();
                break;
            case ValueParameter value:
                if (value.Value is Array array)
                {
                    writer.WriteStartArray();
                    foreach (object? item in array) writer.WriteValue(item);
                    writer.WriteEndArray();
                }
                else writer.WriteValue(value.Value);
                break;
        }
    }
    #endregion
}