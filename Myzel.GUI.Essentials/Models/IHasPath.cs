namespace Myzel.GUI.Essentials.Models;

public interface IHasPath
{
    public string FullPath { get; }
    public string Name { get; set; }
    public bool LoadingFailed { get; set; }
}