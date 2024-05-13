using Myzel.Core.FileTypes;
using FolderData = (Myzel.Core.FileTypes.FileData Folder, System.Collections.Generic.List<Myzel.Core.FileTypes.FileData> Files);
namespace Myzel.Core.Utils;

public static class FolderBuilder
{
    public static ICollection<FileData> Wrap(IEnumerable<FileData> files, string filePath, Func<ICollection<FileData>, string, FileData> folderCreator)
    {
        var fileMap = new Dictionary<string, FolderData>();
        FolderData? lastFolderData = null;
        foreach (var file in files)
        {
            var folderPath = Path.GetDirectoryName(file.FilePath)!.Replace('\\', '/');

            var index = filePath.Length + 1;
            while (index < folderPath.Length)
            {
                index = folderPath.IndexOf('/', index + 1);
                if (index < 0) break;

                lastFolderData = CreateFolder(folderPath[..index], lastFolderData);
            }

            var (folder, fileList) = CreateFolder(folderPath, lastFolderData);
            file.Parent = folder;
            fileList.Add(file);
        }

        var wrappedFiles = new List<FileData>();
        foreach (var (folder, fileList) in fileMap.Values)
        {
            if (folder.FilePath.Length == filePath.Length) wrappedFiles.AddRange(fileList);
            else if (folder.Parent is null) wrappedFiles.Add(folder);
        }
        return wrappedFiles;

        FolderData CreateFolder(string subFolderPath, FolderData? lastFolder)
        {
            if (fileMap.TryGetValue(subFolderPath, out var subFolderData)) return subFolderData;

            var fileList = new List<FileData>();
            var subFolder = folderCreator(fileList, subFolderPath);

            if (lastFolder is not null && subFolderPath.StartsWith(lastFolder.Value.Folder.FilePath))
            {
                subFolder.Parent = lastFolder.Value.Folder;
                lastFolder.Value.Files.Add(subFolder);
            }

            var newFolderData = new FolderData(subFolder, fileList);
            fileMap.Add(subFolderPath, newFolderData);
            return newFolderData;
        }
    }

    public static ICollection<FileData> WrapPhysical(IEnumerable<FileData> files, string filePath)
    {
        return Wrap(files, filePath, (childFiles, folderPath) => new FolderFileData(childFiles) {FilePath = folderPath});
    }

    public static ICollection<FileData> WrapVirtual(IEnumerable<FileData> files, string filePath)
    {
        return Wrap(files, filePath, (childFiles, folderPath) => new VirtualFolderFileData(childFiles) {FilePath = folderPath});
    }
}