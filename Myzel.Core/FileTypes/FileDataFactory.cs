using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Services;
using Myzel.Lib.FileFormats;
using Myzel.Lib.FileFormats.Bmg;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Sarc;
using Myzel.Lib.FileFormats.Umsbt;
using Myzel.Lib.Compression;
using Myzel.Lib.Compression.ZSTD;

namespace Myzel.Core.FileTypes;

public class FileDataFactory(SettingsService settings)
{
    #region private members
    private static readonly ZstdDecompressor ZstdDecompressor = new();
    private static readonly SarcFileParser SarcFileParser = new();
    private static readonly UmsbtFileParser UmsbtFileParser = new();
    private static readonly MsbtFileParser MsbtFileParser = new();
    private static readonly BmgFileParser BmgFileParser = new();
    #endregion

    #region public methods
    public FileData Create(StorageProvider provider, string filePath)
    {
        if (Directory.Exists(filePath))
        {
            return new FolderFileData(this) {FilePath = filePath};
        }

        var stream = provider.GetReadStream();

        //check compression types
        if (ZstdDecompressor.CanDecompress(stream))
        {
            provider = new ZstdCompressionWrapper(provider, settings);
            stream.Close();
            stream = provider.GetReadStream();
        }

        //check file types
        FileData fileData;
        if (SarcFileParser.CanParse(stream))
        {
            fileData = new SarcFileData(provider, this) {FilePath = filePath};
        }
        else if (UmsbtFileParser.CanParse(stream))
        {
            fileData = new UmsbtFileData(provider, this) {FilePath = filePath};
        }
        else if (MsbtFileParser.CanParse(stream))
        {
            fileData = new MsbtFileData(provider, settings) {FilePath = filePath};
        }
        else if (BmgFileParser.CanParse(stream))
        {
            fileData = new BmgFileData(provider, settings) {FilePath = filePath};
        }
        else if (ZipFileData.CanParse(stream))
        {
            fileData = new ZipFileData(provider, this) {FilePath = filePath};
        }
        else
        {
            fileData = new UnknownFileData(provider) {FilePath = filePath};
        }

        stream.Close();
        return fileData;
    }
    #endregion
}