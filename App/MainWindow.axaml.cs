using App.Domain.Models;
using App.Domain.Static;
using App.Pages;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        AppSettings.Load();

        MainContent.Content = Content;
        Content.Content = new MainPage();
    }

    private async void Window_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        await AppSettings.SaveAsync();
    }
}