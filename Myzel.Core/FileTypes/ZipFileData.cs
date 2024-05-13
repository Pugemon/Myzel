using System.IO.Compression;
using System.Text;
using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Utils;

namespace Myzel.Core.FileTypes;

internal class ZipFileData(StorageProvider provider, FileDataFactory fileFactory) : FileData(provider)
{
    #region private members
    private ZipContent[]? _files;
    #endregion

    #region public properties
    public override string Type => "zip";

    public override bool IsContainer => true;
    #endregion

    #region public methods
    public static bool CanParse(Stream stream)
    {
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        return reader.ReadBytes(4) is [0x50, 0x4B, 0x03, 0x04];
    }
    #endregion

    #region protected methods
    protected override async Task InternalLoad(Stream stream)
    {
        using var zipFile = new ZipArchive(stream, ZipArchiveMode.Read);

        var entries = new List<ZipContent>();
        var files = new List<FileData>();
        foreach (var entry in zipFile.Entries)
        {
            await using var entryStream = entry.Open();
            await using var dataStream = new MemoryStream(new byte[entry.Length]);
            await entryStream.CopyToAsync(dataStream);
            var data = dataStream.ToArray();

            var content = new ZipContent
            {
                Name = entry.FullName,
                Data = data
            };

            entries.Add(content);
            if (data.Length == 0) continue; //entry is a folder

            var provider = new VirtualStorageProvider(() => content.Data, data1 => content.Data = data1);
            var file = fileFactory.Create(provider, FilePath + "/" + entry.FullName);

            files.Add(file);
        }

        _files = [..entries];
        Children = FolderBuilder.WrapVirtual(files, FilePath);
    }

    protected override async Task InternalSave(Stream stream)
    {
        if (_files is null) return;

        using var zipFile = new ZipArchive(stream, ZipArchiveMode.Create);

        foreach (var file in _files)
        {
            var entry = zipFile.CreateEntry(file.Name);

            await using var entryStream = entry.Open();
            await using var dataStream = new MemoryStream(file.Data);
            await dataStream.CopyToAsync(entryStream);
        }
    }
    #endregion

    #region helper class
    private class ZipContent
    {
        public required string Name { get; init; }

        public required byte[] Data { get; set; }
    }
    #endregion
}