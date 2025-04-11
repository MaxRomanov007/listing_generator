using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using App.Dialogs;
using App.Domain.Models;
using App.Domain.Utils;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using MsBox.Avalonia.Enums;

namespace App.Pages;

public partial class MainPage : UserControl
{
    private readonly MainPageFields _fields = new();

    public MainPage()
    {
        InitializeComponent();
        ConfigurationsComboBox.ItemsSource = AppSettings.Configurations.Keys;
        _fields.RootPath = AppSettings.Session.RootPath;
        _fields.SelectedConfiguration = AppSettings.Session.SelectedConfiguration;
        DataContext = _fields;
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void SelectFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Выберите папку",
                AllowMultiple = false
            });

            if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } selectedPath)
            {
                _fields.RootPath = selectedPath;
            }
        }
        catch (Exception ex)
        {
            await Domain.Utils.Dialogs.ShowErrorAsync($"Не удалось выбрать папку: {ex.Message}");
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void IncludePatternAddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _fields.ValidateIncludePattern();
        if (_fields.HasErrors) return;

        _fields.IncludePatterns.Add(_fields.IncludePattern);

        _fields.IncludePattern = string.Empty;
    }

    private void IncludePatternTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        IncludePatternAddButton_OnClick(sender, e);
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void DeleteIncludePatternButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not string pattern) return;

        _fields.IncludePatterns.Remove(pattern);
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void ExcludePatternAddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _fields.ValidateExcludePattern();
        if (_fields.HasErrors) return;

        _fields.ExcludePatterns.Add(_fields.ExcludePattern);

        _fields.ExcludePattern = string.Empty;
    }

    private void ExcludePatternTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        ExcludePatternAddButton_OnClick(sender, e);
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void DeleteExcludePatternButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (button.DataContext is not string pattern) return;

        _fields.ExcludePatterns.Remove(pattern);
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void CreateListingButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _fields.ValidateRootPath();
        if (_fields.HasErrors) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить листинг",
            SuggestedFileName = "Листинг.docx",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Docx файл")
                {
                    Patterns = new[] { "*.docx" }
                }
            }
        });

        if (file is null) return;

        _fields.ValidateRootPath();
        if (_fields.HasErrors)
        {
            return;
        }

        var files = FileScanner.ScanFiles(
            _fields.RootPath,
            _fields.IncludePatterns.ToList(),
            _fields.ExcludePatterns.ToList()
        );

        try
        {
            await WordGenerator.GenerateReport(_fields.RootPath, files, file.Path.LocalPath);
            if (AppSettings.Config.Generating.IsOpenAfterSave) return;
            await Domain.Utils.Dialogs.ShowSuccessAsync("Листинг успешно записан");
        }
        catch (IOException)
        {
            await Domain.Utils.Dialogs.ShowErrorAsync("Закройте файл перед загрузкой");
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void SaveConfigurationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new InputConfigurationNameDialog();
        var window = this.FindAncestorOfType<Window>();
        if (window == null) return;

        var result = await dialog.ShowDialog<string>(window);

        if (string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        var configName = result.Trim();

        if (AppSettings.Configurations.ContainsKey(configName))
        {
            var confirmResult = await Domain.Utils.Dialogs.ShowQuestionAsync(
                "Конфигурация с таким именем уже существует. Перезаписать?"
            );

            if (confirmResult != ButtonResult.Yes)
            {
                _fields.SelectedConfiguration = configName;
                return;
            }
        }

        AppSettings.UpdateConfigurations(d => d[configName] = new Patterns
        {
            Include = _fields.IncludePatterns.ToList(),
            Exclude = _fields.ExcludePatterns.ToList()
            });

        _fields.SelectedConfiguration = configName;
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void ConfigurationsComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        if (comboBox.SelectedItem is not string selectedConfiguration) return;

        if (!AppSettings.Configurations.TryGetValue(selectedConfiguration, out var configuration)) return;

        _fields.IncludePatterns.Clear();
        foreach (var pattern in configuration.Include)
        {
            _fields.IncludePatterns.Add(pattern);
        }

        _fields.ExcludePatterns.Clear();
        foreach (var pattern in configuration.Exclude)
        {
            _fields.ExcludePatterns.Add(pattern);
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void DeleteConfigurationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ConfigurationsComboBox.SelectedItem is not string selectedConfiguration) return;
        if (!AppSettings.Configurations.ContainsKey(selectedConfiguration)) return;

        AppSettings.UpdateConfigurations(d => d.Remove(selectedConfiguration));
        _fields.IncludePatterns.Clear();
        _fields.ExcludePatterns.Clear();
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void Page_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        AppSettings.UpdateSession(s =>
        {
            s.RootPath = _fields.RootPath;
            s.SelectedConfiguration = _fields.SelectedConfiguration;
        });
    }

    private async void OpenSettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new ConfigDialog();
        var window = this.FindAncestorOfType<Window>();
        if (window == null) return;

        await dialog.ShowDialog<string>(window);
    }
}