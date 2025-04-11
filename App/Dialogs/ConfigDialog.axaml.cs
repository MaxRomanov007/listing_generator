using App.Domain.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace App.Dialogs;

public partial class ConfigDialog : Window
{
    private GeneratingConfig _config;

    public ConfigDialog()
    {
        InitializeComponent();
        _config = AppSettings.Config.Generating;
        DataContext = _config;
    }

    private void DefaultButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _config = new GeneratingConfig();
        DataContext = _config;
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        AppSettings.UpdateConfig(cfg =>
        {
            cfg.Generating.MarginTop = _config.MarginTop;
            cfg.Generating.MarginRight = _config.MarginRight;
            cfg.Generating.MarginBottom = _config.MarginBottom;
            cfg.Generating.MarginLeft = _config.MarginLeft;

            cfg.Generating.MainFont = _config.MainFont;
            cfg.Generating.CodeFont = _config.CodeFont;

            cfg.Generating.MainFontSize = _config.MainFontSize;
            cfg.Generating.CodeFontSize = _config.CodeFontSize;

            cfg.Generating.MainIndent = _config.MainIndent;
            cfg.Generating.CodeIndent = _config.CodeIndent;

            cfg.Generating.MainLineSpacingMultiplier = _config.MainLineSpacingMultiplier;
            cfg.Generating.CodeLineSpacingMultiplier = _config.CodeLineSpacingMultiplier;

            cfg.Generating.IsIntervalBeforeTitle = _config.IsIntervalBeforeTitle;
            cfg.Generating.IsIntervalAfterTitle = _config.IsIntervalAfterTitle;

            cfg.Generating.IsCodeInTable = _config.IsCodeInTable;
            cfg.Generating.IsOpenAfterSave = _config.IsOpenAfterSave;
        });
        Close();
    }
}