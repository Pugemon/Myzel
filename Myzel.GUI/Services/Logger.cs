using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media;
using Myzel.GUI.Data;
using Myzel.GUI.Essentials.Helpers;
using Myzel.GUI.Essentials.Services;

namespace Myzel.GUI.Services;

public class Logger : ILogger
{
    private static readonly DateTime AppStart = DateTime.Now;

    private static TextWriter? _log;

    private string LogFilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), _paths.AppFolderName,
            "Myzel.log");

    private readonly IPaths _paths;
    public Logger(IPaths paths)
    {
        _paths = paths;
        Init();
    }
    
    public void WriteLogFile(string value)
    {
        DateTimeOffset date = DateTimeOffset.Now;
        if (_log == null) return;
        lock (_log)
        {
            _log.WriteLine($"{date:dd-MMM-yyyy HH:mm:ss.fff}> {value}");
            _log.Flush();
        }
    }
    
    public void Log(object message, ConsoleColor color = default(ConsoleColor), bool writeOutput = false, IBrush? outputBrush = null)
    {
        TimeSpan appRun = DateTime.Now - AppStart;
#if DEBUG
        if(RuntimeInformation.ProcessArchitecture is not Architecture.Wasm) Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(@"[" + $"{(int)appRun.TotalHours:D2}:{(int)appRun.TotalMinutes:D2}:{appRun.Seconds:D2}" + @"] ");
        if(RuntimeInformation.ProcessArchitecture is not Architecture.Wasm) Console.ForegroundColor = color;
        Console.WriteLine(message);
        if(RuntimeInformation.ProcessArchitecture is not Architecture.Wasm) Console.ForegroundColor = default;
#endif
        WriteLogFile(message?.ToString() ?? "");
    }

    public static string CurrentTimeString()
    {
        DateTime time = DateTime.Now;
        return "[" + $"{time.Hour:D2}:{time.Minute:D2}:{time.Second:D2}" + "]";
    }

    public void Error(string message, Exception? exception = null, bool showOutput = true,
        bool showDialog = false, Window? dialogOwner = null)
    {
        Log(message + "\n" + exception, ConsoleColor.Red);
    }

    public void Warning(string message, Exception? exception = null, bool showOutput = true,
        bool showDialog = false, Window? dialogOwner = null)
    {
        Log(message + "\n" + exception, ConsoleColor.Yellow);

        
    }

    private void Init()
    {
        try
        {
            Directory.CreateDirectory(_paths.DocumentsDirectory);
            _log = File.CreateText(LogFilePath);
            PlatformHelper.ChmodFile(LogFilePath);
            
            this.Log($"Version: {Global.VersionCode} OS: {RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}", ConsoleColor.Cyan);
        }
        catch
        {
            Console.WriteLine("Can't create/access log file!");
        }
    }
}