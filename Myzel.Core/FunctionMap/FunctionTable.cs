using System.Text;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Core.FunctionMap;

internal class FunctionTable(FunctionMap map) : IMsbtFunctionTable
{
    public bool BigEndian { get; set; }

    public void GetFunction(MsbtFunction function, bool bigEndian, Encoding encoding, out string functionName, out IEnumerable<MsbtFunctionArgument> functionArgs)
    {
        List<MsbtFunctionArgument> argList = [];
        int argOffset = 0;

        if (map.TryGetFunction(function.Group, function.Type, out var functionInfo))
        {
            functionName = functionInfo.Name;
            if (functionInfo.IsDiscardType || functionInfo.TypeList.Length > 0) functionName += $":{function.Type}";
            else if (functionInfo.TypeMap is not null)
            {
                string searchType = function.Type.ToString();
                FunctionValue value = Array.Find(functionInfo.TypeMap, v => v.Value == searchType) ??
                                      throw new FileFormatException($"Failed to parse function type of \"{functionName}\" from map.");
                functionName += $":{value.Name}";
            }

            foreach (FunctionArg arg in functionInfo.Args)
            {
                //discard padding
                if (arg.IsDiscard)
                {
                    argOffset = function.Args.Length;
                    break;
                }

                //argument padding
                if (arg.IsPadding)
                {
                    argOffset += arg.DataType.Length * (arg.ArrayLength > 0 ? arg.ArrayLength : 1);
                    continue;
                }

                //handle arrays
                if (arg.ArrayLength > 0)
                {
                    object[] arr = new object[arg.ArrayLength];
                    for (int i = 0; i < arg.ArrayLength; ++i)
                    {
                        arr[i] = ParseArgument(arg, functionInfo);
                    }
                    argList.Add(new MsbtFunctionArgument(arg.Name, arr));
                }
                else
                {
                    argList.Add(new MsbtFunctionArgument(arg.Name, ParseArgument(arg, functionInfo)));
                }
            }

            if (argOffset < function.Args.Length)
            {
                argList.Add(new MsbtFunctionArgument("otherArg", function.Args[argOffset..].ToHexString(true)));
            }
        }
        else
        {
            functionName = $"{function.Group}:{function.Type}";
            if (function.Args.Length > 0) argList.Add(new MsbtFunctionArgument("arg", function.Args.ToHexString(true)));
        }

        functionArgs = argList;
        return;

        string ParseArgument(FunctionArg arg, FunctionInfo info)
        {
            try
            {
                (string value, int count) = arg.DataType.Deserialize(function.Args, argOffset, BigEndian, encoding);
                argOffset += count;

                if (arg.ValueMap is null) return value;
                foreach (FunctionValue mapValue in arg.ValueMap)
                {
                    if (mapValue.Value == value) return mapValue.Name;
                }
                return value;
            }
            catch
            {
                throw new FileFormatException($"Failed to parse function argument value of \"{arg.Name}\" on \"{info.Name}\" as {arg.DataType.Name}.");
            }
        }
    }
}