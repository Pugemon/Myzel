using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Myzel.GUI.Views;
using Myzel.GUI.Views.Welcome.Views;
using ReactiveUI;

namespace Myzel.GUI.ViewModels;

public class WelcomeWindowViewModel : ViewModelBase
{
    private object? _mainContentAreaContent = new Home();

    public object? MainContentAreaContent
    {
        get => _mainContentAreaContent ?? null;
        set => this.RaiseAndSetIfChanged(ref _mainContentAreaContent, value);
    }

    public ReactiveCommand<string, Unit> OpenLinkCommand { get; } = ReactiveCommand.Create<string>(OpenLink);
    public ReactiveCommand<string, Unit> ChangeContentCommand { get; }
        

    public WelcomeWindowViewModel()
    {
        ChangeContentCommand = ReactiveCommand.CreateFromObservable<string, Unit>(ChangeContent);
    }

    private IObservable<Unit> ChangeContent(string content)
    {
        switch (content)
        {
            case "Home":
                MainContentAreaContent = new Home();
                break;
            case "Projects":
                // MainContentArea = new ProjectsUserControl();
                MainContentAreaContent = new Projects();
                break;
            case "Dictionaries":
                // MainContentArea = new DictionariesUserControl();
                break;
            default:
                // Можно выбросить исключение или добавить обработку других случаев
                throw new ArgumentException("Unknown content type", nameof(content));
        }
        return Observable.Return(Unit.Default);
    }
    private static void OpenLink(string url)
    {
        ProcessStartInfo info = new()
        {
            FileName = url, UseShellExecute = true
        };
        Process.Start(info);
    }
}

public class ProjectsViewModel : ViewModelBase
{
    public ReactiveCommand<string, Unit> OpenEditorCommand { get; } = ReactiveCommand.Create<string>(OpenEditor);
    private static void OpenEditor(string path)
    {
        // Ваша логика открытия редактора
        Window? window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        window?.Hide();
        EditorWindow newWindow = new();
        newWindow.Closed += (_, _) => window?.Show();
        newWindow.Show();
    }
}
