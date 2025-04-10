using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using App.Domain.Models;
using App.Domain.Utils;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
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
            await MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Ошибка",
                    ContentMessage = $"Не удалось выбрать папку: {ex.Message}",
                    Icon = Icon.Error,
                    WindowIcon = new WindowIcon("Assets/Logo.ico"),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                }
            ).ShowAsync();
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void IncludePatternAddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _fields.ValidateIncludePattern();
        if (_fields.HasErrors)
        {
            return;
        }

        _fields.IncludePatterns.Add(new Pattern
        {
            Text = _fields.IncludePattern,
            Index = _fields.IncludePatterns.Count
        });

        _fields.IncludePattern = string.Empty;
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void DeleteIncludePatternButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not int index)
        {
            return;
        }

        _fields.IncludePatterns.Remove(_fields.IncludePatterns.First(p => p.Index == index));
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void ExcludePatternAddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _fields.ValidateExcludePattern();

        if (_fields.HasErrors)
        {
            return;
        }

        _fields.ExcludePatterns.Add(new Pattern
        {
            Text = _fields.ExcludePattern,
            Index = _fields.ExcludePatterns.Count
        });

        _fields.ExcludePattern = string.Empty;
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void DeleteExcludePatternButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not int index)
        {
            return;
        }

        _fields.ExcludePatterns.Remove(_fields.ExcludePatterns.First(p => p.Index == index));
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void SaveSettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить настройки",
                SuggestedFileName = "settings.json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("JSON файл")
                    {
                        Patterns = new[] { "*.json" }
                    }
                }
            });

            if (file is null) return;

            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);

            var settings = new
            {
                _fields.IncludePatterns,
                _fields.ExcludePatterns
            };

            var json = JsonSerializer.Serialize(settings);
            await writer.WriteAsync(json);
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Ошибка",
                    ContentMessage = $"Не удалось сохранить настройки: {ex.Message}",
                    Icon = Icon.Error,
                    WindowIcon = new WindowIcon("Assets/Logo.ico"),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                }
            ).ShowAsync();
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void LoadSettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Загрузить настройки",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON файл")
                    {
                        Patterns = new[] { "*.json" }
                    }
                }
            });

            if (files.Count == 0) return;

            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var settings = JsonSerializer.Deserialize<MainPageFields>(json);
            if (settings == null) return;

            _fields = settings;
            DataContext = _fields;
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Ошибка",
                    ContentMessage = $"Не удалось загрузить настройки: {ex.Message}",
                    Icon = Icon.Error,
                    WindowIcon = new WindowIcon("Assets/Logo.ico"),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                }
            ).ShowAsync();
        }
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void CreateListingButton_OnClick(object? sender, RoutedEventArgs e)
    {
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
            await WordGenerator.GenerateReport(_fields.RootPath, files, file.Name);
        }
        catch (IOException)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                new MessageBoxStandardParams
                {
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Ошибка",
                    ContentMessage = "Закройте файл перед загрузкой",
                    Icon = Icon.Error,
                    WindowIcon = new WindowIcon("Assets/Logo.ico"),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                }
            ).ShowAsync();
        }
    }
    
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private async void PageControl_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        var settings = new
        {
            _fields.RootPath,
            _fields.IncludePatterns,
            _fields.ExcludePatterns
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

            _fields = settings;
            DataContext = _fields;
        }
        catch (IOException)
        {
            Console.WriteLine("settings file doest exists");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"failed to read settings on load: {ex.Message}");
        }
    }
}