namespace Myzel.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public WelcomeWindowViewModel WelcomeWindow { get; } = new();
}