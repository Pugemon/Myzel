using System.Text;
using System.Text.RegularExpressions;
using Myzel.Core.FunctionMap;
using Myzel.Lib.FileFormats.Bmg;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Core.Utils;

public static partial class TextConverter
{

    #region private members

    private static readonly IMsbtFormatProvider FormatProvider = new MessageFormatProvider();
    private static readonly HeaderParameter<MsbtFile>[] MsbtHeaderParams = GetMsbtHeaderParams();
    private static readonly HeaderParameter<BmgFile>[] BmgHeaderParams = GetBmgHeaderParams();
    private static readonly HeaderParameter<MsbtMessage>[] MsbtMessageHeaderParams = GetMsbtMessageHeaderParams();

    #endregion

    #region public methods

    public static string SerializeMsbt(MsbtFile file, FunctionMap.FunctionMap map)
    {
        StringBuilder content = new();
        FunctionTable table = new(map)
        {
            BigEndian = file.BigEndian
        };

        //msbt header
        content.AppendLine("%%%");
        content.Append("bigEndian: ").AppendLine(file.BigEndian.ToString().ToLowerInvariant());
        content.Append("version: ").AppendLine(file.Version.ToString());
        content.Append("encoding: ").AppendLine(file.Encoding.WebName);
        content.Append("hasNLI1: ").AppendLine(file.HasNli1.ToString().ToLowerInvariant());
        content.Append("hasLBL1: ").AppendLine(file.HasLbl1.ToString().ToLowerInvariant());
        if (file.HasLbl1) content.Append("labelGroups: ").AppendLine(file.LabelGroups.ToString());
        content.Append("hasATR1: ").AppendLine(file.HasAtr1.ToString().ToLowerInvariant());
        if (file.HasAtr1) content.Append("hasAttributeText: ").AppendLine(file.HasAttributeText.ToString().ToLowerInvariant());
        if (file.AdditionalAttributeData.Length > 0) content.Append("additionalAttributeData: ").AppendLine(file.AdditionalAttributeData.ToHexString(true));
        content.Append("hasATO1: ").AppendLine(file.HasAto1.ToString().ToLowerInvariant());
        if (file.HasAto1) content.Append("ATO1Data: ").AppendLine(file.Ato1Data.ToHexString(true));
        content.Append("hasTSY1: ").AppendLine(file.HasTsy1.ToString().ToLowerInvariant());
        content.AppendLine("%%%");

        foreach (MsbtMessage message in file.Messages)
        {
            //message header
            content.AppendLine();
            content.AppendLine("---");
            content.Append("label: ").AppendLine(message.Label);
            if (file.HasNli1) content.Append("index: ").AppendLine(message.Index.ToString());
            if (file.HasAtr1)
            {
                if (file.HasAttributeText) content.Append("attributeText: ").AppendLine(message.AttributeText);
                else content.Append("attribute: ").AppendLine(message.Attribute.ToHexString(true));
            }
            if (file.HasTsy1) content.Append("styleIndex: ").AppendLine(message.StyleIndex.ToString());
            content.AppendLine("---");

            //message content
            //content.AppendLine(message.ToCompiledString(table, FormatProvider, file.Encoding));
            content.AppendLine(message.ToCompiledString(table, FormatProvider, encoding: file.Encoding, bigEndian: file.BigEndian));
        }

        return content.ToString();
    }

    public static MsbtFile DeserializeMsbt(string? content, FunctionMap.FunctionMap map)
    {
        MsbtFile file = new()
        {
            Encoding = Encoding.Unicode, Version = 3
        };

        string[] lines = content.Split('\n');
        int i = 0;

        //parse file header
        ParseHeaderData(lines, ref i, MsbtHeaderParams, file, HeaderDelimiterRegex());

        //parse messages
        for (; i < lines.Length; ++i)
        {
            MsbtMessage message = new()
            {
                Label = string.Empty
            };
            ParseHeaderData(lines, ref i, MsbtMessageHeaderParams, message, MessageDelimiterRegex());
            if (string.IsNullOrEmpty(message.Label)) throw new FileFormatException($"Message is missing label value on line {i + 1}.");

            ParseMessageBody(lines, ref i, message, file.BigEndian, file.Encoding, map);
            file.Messages.Add(message);
        }

        return file;
    }

    public static string SerializeBmg(BmgFile file, FunctionMap.FunctionMap map)
    {
        StringBuilder content = new();
        FunctionTable table = new(map)
        {
            BigEndian = file.BigEndian
        };

        //bmg header
        content.AppendLine("%%%");
        content.Append("bigEndian: ").AppendLine(file.BigEndian.ToString().ToLowerInvariant());
        content.Append("encoding: ").AppendLine(file.Encoding.WebName);
        content.Append("fileId: ").AppendLine(file.FileId.ToString());
        content.Append("defaultColor: ").AppendLine(file.DefaultColor.ToString());
        content.Append("hasMID1: ").AppendLine(file.HasMid1.ToString().ToLowerInvariant());
        if (file.HasMid1) content.Append("MID1Format: ").AppendLine(file.Mid1Format.ToHexString(true));
        content.AppendLine("%%%");

        foreach (MsbtMessage message in file.Messages)
        {
            //message header
            content.AppendLine();
            content.AppendLine("---");
            content.Append("label: ").AppendLine(message.Label);
            if (message.Attribute?.Length > 0) content.Append("attribute: ").AppendLine(message.Attribute.ToHexString(true));
            content.AppendLine("---");

            //message content
            content.AppendLine(message.ToCompiledString(table, FormatProvider, encoding: file.Encoding, bigEndian: file.BigEndian));
        }

        return content.ToString();
    }

    public static BmgFile DeserializeBmg(string? content, FunctionMap.FunctionMap map)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        BmgFile file = new()
        {
            Encoding = Encoding.GetEncoding(1252)
        };

        string[] lines = content.Split('\n');
        int i = 0;

        //parse file header
        ParseHeaderData(lines, ref i, BmgHeaderParams, file, HeaderDelimiterRegex());

        //parse messages
        for (; i < lines.Length; ++i)
        {
            MsbtMessage message = new MsbtMessage
            {
                Label = string.Empty
            };
            ParseHeaderData(lines, ref i, MsbtMessageHeaderParams, message, MessageDelimiterRegex());
            if (string.IsNullOrEmpty(message.Label)) throw new FileFormatException($"Message is missing label value on line {i + 1}.");

            ParseMessageBody(lines, ref i, message, file.BigEndian, file.Encoding, map);
            file.Messages.Add(message);
        }

        return file;
    }

    #endregion

    #region private methods

    private static HeaderParameter<MsbtFile>[] GetMsbtHeaderParams()
    {
        return
        [
            new HeaderParameter<MsbtFile>
            {
                Name = "bigEndian", Parser = (file, value, name, line) => file.BigEndian = ParseBool(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "version", Parser = (file, value, name, line) => file.Version = ParseInt(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "encoding", Parser = (file, value, name, line) => file.Encoding = ParseEncoding(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "hasNLI1", Parser = (file, value, name, line) => file.HasNli1 = ParseBool(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "hasLBL1", Parser = (file, value, name, line) => file.HasLbl1 = ParseBool(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "labelGroups", Parser = (file, value, name, line) => file.LabelGroups = ParseInt(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "hasATR1", Parser = (file, value, name, line) => file.HasAtr1 = ParseBool(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "hasAttributeText", Parser = (file, value, name, line) => file.HasAttributeText = ParseBool(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "additionalAttributeData", Parser = (file, value, name, line) => file.AdditionalAttributeData = ParseHexString(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "hasATO1", Parser = (file, value, name, line) => file.HasAto1 = ParseBool(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "ATO1Data", Parser = (file, value, name, line) => file.Ato1Data = ParseHexString(value, name, line)
            },
            new HeaderParameter<MsbtFile>
            {
                Name = "hasTSY1", Parser = (file, value, name, line) => file.HasTsy1 = ParseBool(value, name, line)
            }
        ];
    }

    private static HeaderParameter<BmgFile>[] GetBmgHeaderParams()
    {
        return
        [
            new HeaderParameter<BmgFile>
            {
                Name = "bigEndian", Parser = (file, value, name, line) => file.BigEndian = ParseBool(value, name, line)
            },
            new HeaderParameter<BmgFile>
            {
                Name = "encoding", Parser = (file, value, name, line) => file.Encoding = ParseEncoding(value, name, line)
            },
            new HeaderParameter<BmgFile>
            {
                Name = "fileId", Parser = (file, value, name, line) => file.FileId = ParseInt(value, name, line)
            },
            new HeaderParameter<BmgFile>
            {
                Name = "defaultColor", Parser = (file, value, name, line) => file.DefaultColor = ParseInt(value, name, line)
            },
            new HeaderParameter<BmgFile>
            {
                Name = "hasMID1", Parser = (file, value, name, line) => file.HasMid1 = ParseBool(value, name, line)
            },
            new HeaderParameter<BmgFile>
            {
                Name = "MID1Format", Parser = (file, value, name, line) => file.Mid1Format = ParseHexString(value, name, line)
            }
        ];
    }

    private static HeaderParameter<MsbtMessage>[] GetMsbtMessageHeaderParams()
    {
        return
        [
            new HeaderParameter<MsbtMessage>
            {
                Name = "label", Parser = (message, value, _, _) => message.Label = value
            },
            new HeaderParameter<MsbtMessage>
            {
                Name = "index", Parser = (message, value, name, line) => message.Index = ParseInt(value, name, line)
            },
            new HeaderParameter<MsbtMessage>
            {
                Name = "attribute", Parser = (message, value, name, line) => message.Attribute = ParseHexString(value, name, line)
            },
            new HeaderParameter<MsbtMessage>
            {
                Name = "attributeText", Parser = (message, value, _, _) => message.AttributeText = value
            },
            new HeaderParameter<MsbtMessage>
            {
                Name = "styleIndex", Parser = (message, value, name, line) => message.StyleIndex = ParseInt(value, name, line)
            }
        ];
    }

    private static void ParseHeaderData<T>(IReadOnlyList<string> lines, ref int i, HeaderParameter<T>[] parameters, T instance, Regex headerDelimiter)
    {
        bool headerStarted = false;

        for (; i < lines.Count; ++i)
        {
            string line = lines[i].TrimEnd('\r');

            if (headerDelimiter.IsMatch(line))
            {
                if (headerStarted)
                {
                    ++i;
                    break;
                }
                headerStarted = true;
                continue;
            }

            if (!headerStarted) continue;

            Match match = HeaderParameterRegex().Match(line);
            if (!match.Success) continue;

            string paramName = match.Groups[1].Value;
            foreach (HeaderParameter<T> parameter in parameters)
            {
                if (!paramName.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)) continue;
                parameter.Parser(instance, match.Groups[2].Value, parameter.Name, i);
            }
        }
    }

    private static bool ParseBool(string value, string parameter, int line)
    {
        if (bool.TryParse(value, out bool val)) return val;
        throw new FileFormatException($"Invalid boolean value for \"{parameter}\" found on line {line + 1}.");
    }

    private static int ParseInt(string value, string parameter, int line)
    {
        if (int.TryParse(value, out int val)) return val;
        throw new FileFormatException($"Invalid integer value for \"{parameter}\" found on line {line + 1}.");
    }

    private static Encoding ParseEncoding(string value, string parameter, int line)
    {
        try
        {
            return Encoding.GetEncoding(value);
        }
        catch
        {
            throw new FileFormatException($"Invalid encoding value for \"{parameter}\" found on line {line + 1}.");
        }
    }

    private static byte[] ParseHexString(string value, string parameter, int line)
    {
        if (value.StartsWith("0x") && value.Length % 2 == 0)
        {
            try
            {
                return DataTypes.HexString.Serialize(value, !BitConverter.IsLittleEndian, Encoding.UTF8);
            }
            catch
            {
                //ignore
            }
        }

        throw new FileFormatException($"Invalid hex string value for \"{parameter}\" found on line {line + 1}.");
    }

    private static void ParseMessageBody(IReadOnlyList<string> lines, ref int i, MsbtMessage message, bool bigEndian, Encoding encoding, FunctionMap.FunctionMap map)
    {
        int encodingWidth = encoding.GetByteCount("\0");

        for (; i < lines.Count; ++i)
        {
            string text = lines[i];

            if (MessageDelimiterRegex().IsMatch(text))
            {
                --i;
                break;
            }

            foreach (Match functionMatch in FunctionRegex().Matches(text))
            {
                MsbtFunction function = new MsbtFunction();

                string functionName = functionMatch.Groups[1].Value;
                bool argsFound = !string.IsNullOrWhiteSpace(functionMatch.Groups[2].Value);
                if (map.TryGetFunction(functionName, out FunctionInfo? mapFunction))
                {
                    function.Group = mapFunction.Group;
                    function.Type = mapFunction.Type;

                    if (mapFunction.IsDiscardType || mapFunction.TypeList.Length > 0 || mapFunction.TypeMap is not null)
                    {
                        string[] parts = functionName.Split(':');
                        if (parts.Length != 2) throw new FileFormatException($"Invalid function name format on line {i + 1}. Function requires type value.");

                        if (mapFunction.IsDiscardType)
                        {
                            if (ushort.TryParse(parts[1], out ushort type)) function.Type = type;
                            else throw new FileFormatException($"Invalid function name format on line {i + 1}. Function is defined as discard type but the type value could not be parsed as u16.");
                        }
                        else if (mapFunction.TypeList.Length > 0)
                        {
                            if (ushort.TryParse(parts[1], out ushort type)) function.Type = type;
                            else throw new FileFormatException($"Invalid function name format on line {i + 1}. Function is defined with a type-range but the type value could not be parsed as u16.");
                        }
                        else
                        {
                            FunctionValue? value = Array.Find(mapFunction.TypeMap!, v => v.Name == parts[1]);
                            if (value is not null) function.Type = ushort.Parse(value.Value);
                            else throw new FileFormatException($"Invalid function name format on line {i + 1}. \"{parts[1]}\" is not a valid value in the defined value map.");
                        }
                    }

                    if (argsFound)
                    {
                        int argLength = 0;
                        List<byte[]> args = new List<byte[]>();

                        (string, string)[] parsedArgs = ParseFunctionArguments(functionMatch.Groups[2].Value);
                        bool[] handledArgs = new bool[parsedArgs.Length];
                        DataType? discardType = null;
                        DataType? lastType = null;
                        foreach (FunctionArg argInfo in mapFunction.Args)
                        {
                            //discard padding
                            if (argInfo.IsDiscard)
                            {
                                discardType = argInfo.DataType;
                                break;
                            }

                            lastType = argInfo.DataType;

                            //argument padding
                            if (argInfo.IsPadding)
                            {
                                byte[] bytes = argInfo.DataType.Serialize(string.Empty, false, Encoding.UTF8);

                                if (argInfo.ArrayLength > 0)
                                {
                                    byte[] arrBytes = new byte[bytes.Length * argInfo.ArrayLength];

                                    for (int j = 0; j < argInfo.ArrayLength; ++j)
                                    {
                                        Buffer.BlockCopy(bytes, 0, arrBytes, j * bytes.Length, bytes.Length);
                                    }

                                    argLength += arrBytes.Length;
                                    args.Add(arrBytes);
                                }
                                else
                                {
                                    argLength += bytes.Length;
                                    args.Add(bytes);
                                }

                                continue;
                            }

                            //find parsed argument
                            string? argValue = null;
                            for (int j = 0; j < parsedArgs.Length; ++j)
                            {
                                if (argInfo.Name.Equals(parsedArgs[j].Item1, StringComparison.OrdinalIgnoreCase))
                                {
                                    argValue = parsedArgs[j].Item2;
                                    handledArgs[j] = true;
                                    break;
                                }
                            }
                            if (argValue is null) throw new FileFormatException($"Missing argument value for {argInfo.Name} on line {i + 1}.");

                            //handle arrays
                            if (argInfo.ArrayLength > 0)
                            {
                                string[] entries = argValue.TrimStart('[').TrimEnd(']').Split(',');
                                if (argInfo.ArrayLength != entries.Length) throw new FileFormatException($"Argument array lengths do not match on line {i + 1}. Expected an array of length {argInfo.ArrayLength} but found an array of length {entries.Length}.");

                                foreach (string arrValue in entries)
                                {
                                    byte[] bytes = ParseKnownArgumentValue(arrValue.Trim(), argInfo, bigEndian, encoding, i);
                                    argLength += bytes.Length;
                                    args.Add(bytes);
                                }
                            }
                            else
                            {
                                byte[] bytes = ParseKnownArgumentValue(argValue, argInfo, bigEndian, encoding, i);
                                argLength += bytes.Length;
                                args.Add(bytes);
                            }
                        }

                        //append all remaining undefined arguments
                        bool hasUnhandledArgs = false;
                        for (int j = 0; j < parsedArgs.Length; ++j)
                        {
                            if (handledArgs[j]) continue;
                            byte[] bytes = ParseArgumentValue(parsedArgs[j].Item2, DataTypes.HexString, bigEndian, Encoding.UTF8, parsedArgs[j].Item1, i);
                            argLength += bytes.Length;
                            args.Add(bytes);
                            hasUnhandledArgs = true;
                        }

                        //trim last null-string byte
                        if (!hasUnhandledArgs && lastType?.Name == DataTypes.NullString.Name)
                        {
                            args[^1] = args[^1][..^encodingWidth];
                            argLength -= encodingWidth;
                        }

                        //append discard bytes
                        if (discardType is not null)
                        {
                            int padding = (-argLength % encodingWidth + encodingWidth) % encodingWidth;
                            if (padding > 0)
                            {
                                byte[] bytes = new byte[padding];
                                byte discardByte = discardType.Serialize(string.Empty, false, Encoding.UTF8)[0];
                                if (discardByte > 0) Array.Fill(bytes, discardByte);

                                argLength += padding;
                                args.Add(bytes);
                            }
                        }

                        //combine all arguments into single byte array
                        byte[] argBytes = new byte[argLength];
                        int argIndex = 0;
                        foreach (byte[] arg in args)
                        {
                            Buffer.BlockCopy(arg, 0, argBytes, argIndex, arg.Length);
                            argIndex += arg.Length;
                        }

                        function.Args = argBytes;
                    }
                }
                else if (functionName.IndexOf(':') > -1)
                {
                    string[] functionParts = functionName.Split(':');
                    if (!ushort.TryParse(functionParts[0], out ushort group)) throw new FileFormatException($"Invalid function format value for group index found on line {i + 1}.");
                    if (!ushort.TryParse(functionParts[1], out ushort type)) throw new FileFormatException($"Invalid function format value for type index found on line {i + 1}.");
                    function.Group = group;
                    function.Type = type;

                    if (argsFound)
                    {
                        (string, string)[] argParts = ParseFunctionArguments(functionMatch.Groups[2].Value);
                        if (argParts.Length > 1) throw new FileFormatException($"Undefined functions can only have a single argument (error on line {i + 1}).");
                        function.Args = ParseHexString(argParts[0].Item2, argParts[0].Item1, i);
                    }
                }
                else throw new FileFormatException($"Unknown function \"{functionName}\" found on line {i + 1}. If the function is not listed in the used function map, the function has to follow the format of \"<groupIndex>:<typeIndex>\", example \"1:2\".");

                int firstIndex = text.IndexOf(functionMatch.Groups[0].Value, StringComparison.InvariantCulture);
                text = text[..firstIndex] + "{{" + message.Functions.Count + "}}" + text[(firstIndex + functionMatch.Groups[0].Value.Length)..];
                message.Functions.Add(function);
            }

            message.Text += text + "\n";
        }

        if (message.Text.Length == 0) return;
        message.Text = message.Text.EndsWith("\n\n") ? message.Text[..^2] : message.Text[..^1];
    }

    private static (string, string)[] ParseFunctionArguments(string value)
    {
        List<(string, string)> args = new List<(string, string)>();
        foreach (Match match in FunctionArgumentRegex().Matches(value))
        {
            args.Add((match.Groups[1].Value, match.Groups[2].Value));
        }
        return [..args];
    }

    private static byte[] ParseKnownArgumentValue(string value, FunctionArg info, bool isBigEndian, Encoding encoding, int line)
    {
        if (info.ValueMap is null) return ParseArgumentValue(value, info.DataType, isBigEndian, encoding, info.Name, line);

        FunctionValue? val = Array.Find(info.ValueMap, v => v.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (val is not null) return info.DataType.Serialize(val.Value, isBigEndian, encoding);

        throw new FileFormatException($"Failed to find value \"{value}\" of argument \"{info.Name}\" in defined value map (line {line + 1}).");
    }

    private static byte[] ParseArgumentValue(string value, DataType dataType, bool isBigEndian, Encoding encoding, string name, int line)
    {
        try
        {
            return dataType.Serialize(value, isBigEndian, encoding);
        }
        catch
        {
            throw new FileFormatException($"Failed to convert function argument \"{name}\" to \"{dataType.Name}\" (line {line + 1}).");
        }
    }

    #endregion

    #region compiled regex

    [GeneratedRegex("^%%%+")]
    private static partial Regex HeaderDelimiterRegex();

    [GeneratedRegex(@"([A-Za-z0-9_]+)\s*:\s*([A-Za-z0-9_\-]+)")]
    private static partial Regex HeaderParameterRegex();

    [GeneratedRegex("^---+")]
    private static partial Regex MessageDelimiterRegex();

    [GeneratedRegex("""\{\{\s*([A-Za-z0-9_]+:[A-Za-z0-9_]+|[A-Za-z0-9_]+)\s*((?:[A-Za-z0-9_]+=\"[^"]*\"\s*)*)\}\}""")]
    private static partial Regex FunctionRegex();

    [GeneratedRegex("""([A-Za-z0-9_]+)=\"([^"]*)\"\s*""")]
    private static partial Regex FunctionArgumentRegex();

    #endregion

    #region helper class

    private class HeaderParameter<T>
    {
        public required string Name { get; init; }

        public required Action<T, string, string, int> Parser { get; init; }
    }

    #endregion

}