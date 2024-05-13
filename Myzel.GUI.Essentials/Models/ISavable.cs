namespace Myzel.GUI.Essentials.Models;

public interface ISavable : IHasPath
{
    public DateTime LastSaveTime { get; set; }
}