using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
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
    private MainPageFields _fields = new();

    public MainPage()
    {
        InitializeComponent();
        DataContext = _fields;
    }

    private const string SettingsPath = "./settings.json";

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

        _fields.IncludePatterns.Add(new Pattern
        {
            Text = _fields.IncludePattern,
            Index = _fields.IncludePatterns.Count
        });

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
        if (button.Tag is not int index) return;

        _fields.IncludePatterns.Remove(_fields.IncludePatterns.First(p => p.Index == index));
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void ExcludePatternAddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _fields.ValidateExcludePattern();
        if (_fields.HasErrors) return;

        _fields.ExcludePatterns.Add(new Pattern
        {
            Text = _fields.ExcludePattern,
            Index = _fields.ExcludePatterns.Count
        });

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
        if (button.Tag is not int index) return;

        _fields.ExcludePatterns.Remove(_fields.ExcludePatterns.First(p => p.Index == index));
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
            _fields.IncludePatterns.Select(p => p.Text).ToList(),
            _fields.ExcludePatterns.Select(p => p.Text).ToList()
        );

        try
        {
            await WordGenerator.GenerateReport(_fields.RootPath, files, file.Path.LocalPath);
            await Domain.Utils.Dialogs.ShowSuccessAsync("Листинг успешно записан");
        }
        catch (IOException)
        {
            await Domain.Utils.Dialogs.ShowErrorAsync("Закройте файл перед загрузкой");
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void PageControl_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        var settings = new
        {
            _fields.RootPath,
            _fields.Configurations,
            _fields.SelectedConfiguration
        };
        try
        {
            await File.WriteAllTextAsync(SettingsPath, JsonSerializer.Serialize(settings));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed to save settings on unload: {ex.Message}");
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void PageControl_OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            var json = await File.ReadAllTextAsync(SettingsPath);

            var settings = JsonSerializer.Deserialize<MainPageFields>(json);
            if (settings == null) return;

            settings.ConfigurationsNames = new ObservableCollection<string>(settings.Configurations.Keys);

            _fields = settings;
            DataContext = _fields;
        }
        catch (IOException)
        {
            Console.WriteLine("settings file doest exists");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"failed to read settings on load: {ex.Message}");
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

        if (_fields.Configurations.ContainsKey(configName))
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

        _fields.Configurations[configName] = new Patterns
        {
            Include = _fields.IncludePatterns.ToList(),
            Exclude = _fields.ExcludePatterns.ToList()
        };

        if (!_fields.ConfigurationsNames.Contains(configName))
        {
            _fields.ConfigurationsNames.Add(configName);
        }

        _fields.SelectedConfiguration = configName;
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void ConfigurationsComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        if (comboBox.SelectedItem is not string selectedConfiguration) return;

        if (!_fields.Configurations.TryGetValue(selectedConfiguration, out var configuration)) return;

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
        if (!_fields.Configurations.ContainsKey(selectedConfiguration)) return;

        _fields.Configurations.Remove(selectedConfiguration);
        _fields.IncludePatterns.Clear();
        _fields.ExcludePatterns.Clear();

        _fields.ConfigurationsNames.Remove(selectedConfiguration);
    }
}