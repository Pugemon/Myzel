using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Myzel.GUI.ViewModels;

namespace Myzel.GUI.Views.Welcome.Views;

public partial class Projects : UserControl
{
    public Projects()
    {
        InitializeComponent();
        DataContext = new ProjectsViewModel();
    }
}