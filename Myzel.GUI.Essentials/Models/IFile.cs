namespace Myzel.GUI.Essentials.Models;

public interface IFile : ISavable
{
    public string Extension { get; }
}