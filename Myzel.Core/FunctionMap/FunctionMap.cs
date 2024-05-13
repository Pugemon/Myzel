using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Core.FunctionMap;

public sealed partial class FunctionMap : IEnumerable<FunctionInfo>
{
    #region private members
    private static readonly IReadOnlyDictionary<string, DataType> TypeMap = DataTypes.GetTypeList();
    private readonly FunctionInfo[] _map;
    private readonly SortedDictionary<ushort, MapEntry> _optimizedMap;
    #endregion

    #region constructor
    private FunctionMap(FunctionInfo[] map, SortedDictionary<ushort, MapEntry> optimizedMap)
    {
        _map = map;
        _optimizedMap = optimizedMap;
    }
    #endregion

    #region public properties
    public int Count => _map.Length;
    #endregion

    #region public methods
    public static FunctionMap Parse(string? content)
    {
        List<(FunctionInfo, int)> uncheckedMap = [];
        Dictionary<string, (DataType, FunctionValue[])>? valueMaps = new();

        bool functionSection = false;
        bool mapSection = false;
        FunctionInfo? currentFunction = null;
        IList<FunctionArg>? currentArgs = null;
        bool discardDefined = false;
        string? currentValueMap = null;
        IList<FunctionValue>? currentValues = null;
        Dictionary<int, (object, string)> mapReferences = [];

        string[] lines = content.Split('\n');
        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;

            if (FunctionRegex().IsMatch(line, out var functionMatch))
            {
                ResolveOpenElements();
                functionSection = true;
                mapSection = false;
                discardDefined = false;

                ushort type = 0;
                ushort[] typeList = [];
                var isDiscardType = functionMatch.Groups[2].Value == "_";
                string? mapName = null;
                if (!isDiscardType &&
                    !TryGetTypeList(functionMatch.Groups[2].Value, out typeList) &&
                    !TryGetMapName(functionMatch.Groups[2].Value, out mapName))
                {
                    type = ushort.Parse(functionMatch.Groups[2].Value);
                }

                var group = ushort.Parse(functionMatch.Groups[1].Value);
                var name = functionMatch.Groups[3].Success ? functionMatch.Groups[3].Value : isDiscardType || typeList.Length > 0 || mapName is not null ? group.ToString() : $"{group}:{type}";
                var description = functionMatch.Groups[4].Success ? functionMatch.Groups[4].Value.Trim() : string.Empty;

                currentFunction = new FunctionInfo
                {
                    Group = group,
                    Type = typeList.Length > 0 ? typeList[0] : type,
                    TypeList = typeList,
                    IsDiscardType = isDiscardType,
                    Name = name,
                    Description = description
                };
                uncheckedMap.Add((currentFunction, i));

                currentArgs = new List<FunctionArg>();
                if (mapName is not null) mapReferences.Add(i, (currentFunction, mapName));

                continue;
            }

            if (functionSection && ArgumentRegex().IsMatch(line, out var argumentMatch))
            {
                if (discardDefined) throw new FileFormatException($"Argument discard padding must be the last argument in a function declaration (line {i+1}).");

                var dataType = DataTypes.Bool;
                var isPadding = false;
                if (!TryGetMapName(argumentMatch.Groups[1].Value, out var mapName) && !TypeMap.TryGetValue(argumentMatch.Groups[1].Value, out dataType))
                {
                    if (ArgumentPaddingRegex().IsMatch(argumentMatch.Groups[1].Value))
                    {
                        dataType = DataTypes.GetPadding(argumentMatch.Groups[1].Value);
                        isPadding = true;
                    }
                    else if (ArgumentDiscardPaddingRegex().IsMatch(argumentMatch.Groups[1].Value, out var discardMatch))
                    {
                        if (argumentMatch.Groups[2].Success) throw new FileFormatException($"Argument discard padding cannot be an array (line {i+1}).");

                        dataType = DataTypes.GetPadding(discardMatch.Groups[1].Success ? discardMatch.Groups[1].Value : "0x00");
                        isPadding = true;
                        discardDefined = true;
                    }
                    else throw new FileFormatException($"Unknown argument data type \"{argumentMatch.Groups[1].Value}\" on line {i+1}.");
                }

                var arrayLength = argumentMatch.Groups[2].Success ? int.Parse(argumentMatch.Groups[2].Value) : 0;
                var name = argumentMatch.Groups[3].Success ? argumentMatch.Groups[3].Value : $"arg{currentArgs!.Count + 1}";
                var description = argumentMatch.Groups[4].Success ? argumentMatch.Groups[4].Value : string.Empty;

                var arg = new FunctionArg
                {
                    Name = name,
                    Description = description,
                    DataType = dataType,
                    IsPadding = isPadding,
                    IsDiscard = discardDefined,
                    ArrayLength = arrayLength
                };

                if (!currentArgs!.TryAdd(arg, a => a.IsPadding && isPadding || a.Name != name))
                {
                    throw new FileFormatException($"A argument declaration with the same name already exists ({i+1}).");
                }

                if (mapName is not null) mapReferences.Add(i, (arg, mapName));

                continue;
            }

            if (MapRegex().IsMatch(line, out var mapMatch))
            {
                ResolveOpenElements();
                functionSection = false;
                mapSection = true;

                currentValueMap = mapMatch.Groups[1].Value;
                //var description = mapMatch.Groups[3].Success ? mapMatch.Groups[3].Value.Trim() : string.Empty; //unused for now

                var dataType = DataTypes.UInt16;
                if (mapMatch.Groups[2].Success && !TypeMap.TryGetValue(mapMatch.Groups[2].Value, out dataType))
                {
                    throw new FileFormatException($"Unknown map data type \"{mapMatch.Groups[2].Value}\" on line {i+1}.");
                }

                if (!valueMaps.TryAdd(currentValueMap, (dataType, [])))
                {
                    throw new FileFormatException($"A map declaration with the same name already exists (line {i+1}).");
                }

                currentValues = new List<FunctionValue>();

                continue;
            }

            if (mapSection && MapValueRegex().IsMatch(line, out var mapValueMatch))
            {
                var value = mapValueMatch.Groups[1].Value;
                var name = mapValueMatch.Groups[2].Success ? mapValueMatch.Groups[2].Value : value;
                var description = mapValueMatch.Groups[3].Success ? mapValueMatch.Groups[3].Value : string.Empty;

                var datatype = valueMaps[currentValueMap!].Item1;
                try
                {
                    datatype.Serialize(value, !BitConverter.IsLittleEndian, Encoding.UTF8);
                }
                catch
                {
                    throw new FileFormatException($"Failed to convert map value \"{value}\" to \"{datatype.Name}\" (line {i+1}).");
                }

                var item = new FunctionValue
                {
                    Value = value,
                    Name = name,
                    Description = description
                };

                if (!currentValues!.TryAdd(item, v => v.Value != value && v.Name != name))
                {
                    throw new FileFormatException($"A map item with the same name or value already exists ({i+1}).");
                }

                continue;
            }

            if (functionSection) throw new FileFormatException($"Invalid argument format on line {i+1}.");
            if (mapSection) throw new FileFormatException($"Invalid map item format on line {i+1}.");
            throw new FileFormatException($"Unrecognized value found on line {i+1}");
        }

        ResolveOpenElements();

        //resolve map references
        foreach ((int line, (object reference, string mapName)) in mapReferences)
        {
            if (!valueMaps.TryGetValue(mapName, out (DataType, FunctionValue[]) valueMap))
            {
                throw new FileFormatException($"Value map \"{mapName}\" is not defined (line {line+1}).");
            }

            switch (reference)
            {
                case FunctionInfo when valueMap.Item1.Name != DataTypes.UInt16.Name:
                    throw new FileFormatException($"Value maps used for function types must be \"u16\" (line {line+1}).");
                case FunctionInfo info:
                    info.TypeMap = valueMap.Item2;
                    break;
                case FunctionArg arg:
                    arg.DataType = valueMap.Item1;
                    arg.ValueMap = valueMap.Item2;
                    break;
            }
        }

        //check valid map once everything else was resolved
        var map = new List<FunctionInfo>();
        foreach ((FunctionInfo info, int line) in uncheckedMap) TryAddFunction(map, info, line);

        return new FunctionMap([..map], BuildOptimizedMap(map));

        void ResolveOpenElements()
        {
            if (currentFunction is not null)
            {
                currentFunction.Args = currentArgs?.ToArray() ?? [];

                currentFunction = null;
                currentArgs = null;
            }

            if (currentValueMap is not null)
            {
                valueMaps[currentValueMap] = (valueMaps[currentValueMap].Item1, currentValues?.ToArray() ?? []);

                currentValueMap = null;
                currentValues = null;
            }
        }
    }

    public bool TryGetFunction(string name, [MaybeNullWhen(false)] out FunctionInfo function)
    {
        var parts = name.Split(':');
        ushort type = 0;
        var hasGroupNumber = ushort.TryParse(parts[0], out var group);
        var hasTypeNumber = parts.Length > 1 && ushort.TryParse(parts[1], out type);

        if (hasGroupNumber && hasTypeNumber) return TryGetFunction(group, type, out function);

        foreach ((ushort groupId, MapEntry entry) in _optimizedMap)
        {
            if (parts.Length > 1)
            {
                if (hasGroupNumber) //<id>:<map>
                {
                    if (groupId != group) continue;

                    foreach (var info in entry.Types.Values)
                    {
                        if (info.TypeMap is null) continue;
                        foreach (var valueInfo in info.TypeMap)
                        {
                            if (!valueInfo.Name.Equals(parts[1], StringComparison.OrdinalIgnoreCase)) continue;
                            function = info;
                            return true;
                        }
                    }
                }
                else if (hasTypeNumber) //<name>:<id>
                {
                    if (entry.Types.TryGetValue(type, out var info) && info.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase))
                    {
                        function = info;
                        return true;
                    }

                    if (entry.DiscardType?.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase) == true)
                    {
                        function = entry.DiscardType;
                        return true;
                    }
                }
                else //<name>:<map>
                {
                    foreach (var info in entry.Types.Values)
                    {
                        if (!info.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase) || info.TypeMap is null) continue;
                        foreach (var valueInfo in info.TypeMap)
                        {
                            if (!valueInfo.Name.Equals(parts[1], StringComparison.OrdinalIgnoreCase)) continue;
                            function = info;
                            return true;
                        }
                    }
                }
            }
            else //<name>
            {
                foreach (var info in entry.Types.Values)
                {
                    if (info.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        function = info;
                        return true;
                    }
                }
            }
        }

        function = null;
        return false;
    }

    public bool TryGetFunction(int group, int type, [MaybeNullWhen(false)] out FunctionInfo function)
    {
        function = null;

        if (!_optimizedMap.TryGetValue((ushort) group, out var entry)) return false;
        if (entry.Types.TryGetValue((ushort) type, out function)) return true;

        function = entry.DiscardType;
        return entry.DiscardType is not null;
    }
    #endregion

    #region private methods
    private static bool TryGetTypeList(string input, out ushort[] typeList)
    {
        typeList = [];
        if (!input.Contains('-') && (!input.StartsWith('(') || !input.EndsWith(')'))) return false;

        var types = new HashSet<ushort>();

        foreach (var item in input.TrimStart('(').TrimEnd(')').Split(','))
        {
            var bounds = item.Split('-');

            var lowerBound = ushort.Parse(bounds[0]);
            types.Add(lowerBound);
            if (bounds.Length == 1) continue;

            var upperBound = ushort.Parse(bounds[1]);
            for (var i = lowerBound; i <= upperBound; ++i) types.Add(i);
        }

        typeList = [..types.Order()];
        return true;
    }

    private static bool TryGetMapName(string input, [MaybeNullWhen(false)] out string name)
    {
        name = null;
        if (!input.StartsWith('{') || !input.EndsWith('}')) return false;

        name = input[1..^1];
        return true;
    }

    private static void TryAddFunction(ICollection<FunctionInfo> map, FunctionInfo functionInfo, int line)
    {
        foreach (var info in map)
        {
            //same name is only allowed within the same group
            if (info.Group != functionInfo.Group && info.Name == functionInfo.Name)
            {
                throw new FileFormatException($"A function declaration with the same name already exists (line {line+1}).");
            }

            //functions are matched by group
            if (info.Group != functionInfo.Group) continue;

            //can only have one discard per group
            if (info.IsDiscardType && functionInfo.IsDiscardType)
            {
                throw new FileFormatException($"A function group can only have one discard type (line {line+1}).");
            }

            //can only have one static function with identical name
            if (info.Name == functionInfo.Name &&
                info.IsDiscardType == functionInfo.IsDiscardType &&
                info.TypeList.Length == 0 && functionInfo.TypeList.Length == 0 &&
                info.TypeMap is null && functionInfo.TypeMap is null)
            {
                throw new FileFormatException($"Found two non-discard, non-ranged, non-mapped function declarations with the same name (line {line+1}).");
            }

            //type range/map checks
            if (info.TypeList.Length > 0)
            {
                if (functionInfo.TypeList.Length > 0)
                {
                    foreach (var entry in functionInfo.TypeList)
                    {
                        if (TypeListContains(info.TypeList, entry))
                        {
                            throw new FileFormatException($"The type value \"{entry}\" is already defined in another mapped type definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (functionInfo.TypeMap is not null)
                {
                    foreach (var entry in functionInfo.TypeMap)
                    {
                        if (TypeListContains(info.TypeList, ushort.Parse(entry.Value)))
                        {
                            throw new FileFormatException($"The mapped type value \"{entry.Value}\" is already defined in another mapped type definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (TypeListContains(info.TypeList, functionInfo.Type))
                {
                    throw new FileFormatException($"The type value \"{functionInfo.Type}\" is already defined in another mapped type definition within this group (line {line+1}).");
                }
            }
            else if (info.TypeMap is not null)
            {
                if (functionInfo.TypeList.Length > 0)
                {
                    foreach (var entry in functionInfo.TypeList)
                    {
                        if (TypeMapContains(info.TypeMap, entry.ToString()))
                        {
                            throw new FileFormatException($"The type value \"{entry}\" is already defined in another mapped type definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (functionInfo.TypeMap is not null)
                {
                    foreach (var entry in functionInfo.TypeMap)
                    {
                        if (TypeMapContains(info.TypeMap, entry.Value))
                        {
                            throw new FileFormatException($"The mapped type value \"{entry.Value}\" is already defined in another mapped type definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (TypeMapContains(info.TypeMap, functionInfo.Type.ToString()))
                {
                    throw new FileFormatException($"The type value \"{functionInfo.Type}\" is already defined in another mapped type definition within this group (line {line+1}).");
                }
            }
            if (functionInfo.TypeList.Length > 0)
            {
                if (info.TypeList.Length > 0)
                {
                    foreach (var entry in info.TypeList)
                    {
                        if (TypeListContains(functionInfo.TypeList, entry))
                        {
                            throw new FileFormatException($"The type value \"{entry}\" is already defined in another mapped type definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (info.TypeMap is not null)
                {
                    foreach (var entry in info.TypeMap)
                    {
                        if (TypeListContains(functionInfo.TypeList, ushort.Parse(entry.Value)))
                        {
                            throw new FileFormatException($"The mapped type value \"{entry.Value}\" is already defined in another mapped type definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (TypeListContains(functionInfo.TypeList, info.Type))
                {
                    throw new FileFormatException($"The type value \"{info.Type}\" is already defined in another mapped type definition within this group (line {line+1}).");
                }
            }
            else if (functionInfo.TypeMap is not null)
            {
                if (info.TypeList.Length > 0)
                {
                    foreach (var entry in info.TypeList)
                    {
                        if (TypeMapContains(functionInfo.TypeMap, entry.ToString()))
                        {
                            throw new FileFormatException($"The mapped type value \"{entry}\" is already defined in another type-range definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (info.TypeMap is not null)
                {
                    foreach (var entry in info.TypeMap)
                    {
                        if (TypeMapContains(functionInfo.TypeMap, entry.Value))
                        {
                            throw new FileFormatException($"The mapped type value \"{entry.Value}\" is already defined in another mapped type definition within this group (line {line+1}).");
                        }
                    }
                }
                else if (TypeMapContains(functionInfo.TypeMap, info.Type.ToString()))
                {
                    throw new FileFormatException($"The mapped type value \"{info.Type}\" is already defined in another function definition within this group (line {line+1}).");
                }
            }
        }

        map.Add(functionInfo);
        return;

        bool TypeListContains(ushort[] typeList, ushort value) => Array.IndexOf(typeList, value) != -1;
        bool TypeMapContains(IEnumerable<FunctionValue> typeMap, string value) => typeMap.Any(entry => entry.Value == value);
    }

    private static SortedDictionary<ushort, MapEntry> BuildOptimizedMap(IEnumerable<FunctionInfo> map)
    {
        var optimizedMap = new SortedDictionary<ushort, MapEntry>();

        foreach (var info in map)
        {
            if (!optimizedMap.TryGetValue(info.Group, out var entry))
            {
                entry = new MapEntry();
                optimizedMap.Add(info.Group, entry);
            }

            if (info.IsDiscardType)
            {
                entry.DiscardType = info;
            }
            else if (info.TypeList.Length > 0)
            {
                foreach (var item in info.TypeList)
                {
                    entry.Types.Add(item, info);
                }
            }
            else if (info.TypeMap is not null)
            {
                foreach (var item in info.TypeMap)
                {
                    entry.Types.Add(ushort.Parse(item.Value), info);
                }
            }
            else
            {
                entry.Types.Add(info.Type, info);
            }
        }

        return optimizedMap;
    }
    #endregion

    #region compiled regex
    [GeneratedRegex(@"^\[\s*(\d+)\s*,\s*(\d+|\d+\s*-\s*\d+|_|\(\s*\d+(?:\s*-\s*\d+)?(?:\s*,\s*\d+(?:\s*-\s*\d+)?)*\s*\)|\{[A-Za-z0-9_]+\})\s*\]\s*(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.+))?$")]
    private static partial Regex FunctionRegex();

    [GeneratedRegex(@"^\s{2,}([A-Za-z0-9_]+|\{[A-Za-z0-9_]+\})(?:\[(\d+)\])?\s*(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.+))?$")]
    private static partial Regex ArgumentRegex();

    [GeneratedRegex(@"^0x(?:[A-Fa-f0-9]{2})+$")]
    private static partial Regex ArgumentPaddingRegex();

    [GeneratedRegex(@"^_(0x[A-Fa-f0-9]{2})?$")]
    private static partial Regex ArgumentDiscardPaddingRegex();

    [GeneratedRegex(@"^map\s+([A-Za-z0-9_]+)(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.+))?$")]
    private static partial Regex MapRegex();

    [GeneratedRegex(@"^\s{2,}(-?[A-Za-z0-9_]+)(?:\s+([A-Za-z0-9_]+))?\s*(?:\s+#\s*(.+))?$")]
    private static partial Regex MapValueRegex();
    #endregion

    #region helper class
    private class MapEntry
    {
        public SortedDictionary<ushort, FunctionInfo> Types { get; } = [];

        public FunctionInfo? DiscardType { get; set; }
    }
    #endregion

    #region IEnumerable interface
    public IEnumerator<FunctionInfo> GetEnumerator() => ((IEnumerable<FunctionInfo>) _map).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}