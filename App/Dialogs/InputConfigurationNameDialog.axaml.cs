using App.Domain.Models;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace App.Dialogs;

public partial class InputConfigurationNameDialog : Window
{
    private InputDialogContent _content = new();

    public InputConfigurationNameDialog()
    {
        InitializeComponent();
        DataContext = _content;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _content.ValidateText();
        if (_content.HasErrors) return;

        Close(_content.Text);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(string.Empty);
    }

    private void InputTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        OkButton_OnClick(sender, e);
    }
}