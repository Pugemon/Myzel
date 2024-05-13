using Avalonia.Media;

namespace Myzel.GUI.Essentials.Models;

public interface ICanHaveIcon
{
    public IImage? Icon { get; }
}