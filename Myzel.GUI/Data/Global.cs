using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia.Platform.Storage;

namespace Myzel.GUI.Data;

public static class Global
    {
        public static string VersionCode => Assembly.GetEntryAssembly()!.GetName().Version?.ToString() ?? "";

        #region FileDialogFilters

        public static readonly FilePickerFileType VhdpProjFile = new("Project Files (*.myzelproj)")
        {
            Patterns = new[] { "*.myzelproj" }
        };
        

        public static readonly FilePickerFileType ExeFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new FilePickerFileType("Executable (*.exe)")
            {
                Patterns = new[] { "*.exe" }
            } : new FilePickerFileType("Executable (*)") 
            {
                Patterns = new[] { "*.*" }
            };
        
        public static readonly FilePickerFileType AllFiles = new("All files (*)")
        {
            Patterns = new[] { "*.*" }
        };

        #endregion
    }