using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace App.Domain.Models;

public class MainPageFields : INotifyPropertyChanged, INotifyDataErrorInfo
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private readonly Dictionary<string, List<string>> _errors = new();

    private string _rootPath = string.Empty;

    public string RootPath
    {
        get => _rootPath;
        set
        {
            if (_rootPath == value) return;
            _rootPath = value;
            OnPropertyChanged();
            ValidateRootPath();
        }
    }

    private string _includePattern = string.Empty;

    public string IncludePattern
    {
        get => _includePattern;
        set
        {
            if (_includePattern == value) return;
            _includePattern = value;
            OnPropertyChanged();
            ClearErrors(nameof(IncludePattern));
        }
    }

    private string _excludePattern = string.Empty;

    public string ExcludePattern
    {
        get => _excludePattern;
        set
        {
            if (_excludePattern == value) return;
            _excludePattern = value;
            OnPropertyChanged();
            ClearErrors(nameof(ExcludePattern));
        }
    }

    public ObservableCollection<Pattern> IncludePatterns { get; set; } = [];
    public void UpdateIncludePatternsIndexes()
    {
        for (var i = 0; i < IncludePatterns.Count; i++)
        {
            IncludePatterns[i].Index = i;
        }
    }
    
    public ObservableCollection<Pattern> ExcludePatterns { get; set; } = [];
    public void UpdateExcludePatternsIndexes()
    {
        for (var i = 0; i < ExcludePatterns.Count; i++)
        {
            ExcludePatterns[i].Index = i;
        }
    }

    public ObservableCollection<string> ConfigurationsNames { get; set; } = [];

    private string _selectedConfiguration = string.Empty;

    public string SelectedConfiguration
    {
        get => _selectedConfiguration;
        set
        {
            if (_selectedConfiguration == value) return;
            _selectedConfiguration = value;
            OnPropertyChanged();
        }
    }

    public Dictionary<string, Patterns> Configurations { get; set; } = new();

    public void ValidateRootPath()
    {
        ClearErrors(nameof(RootPath));

        if (string.IsNullOrWhiteSpace(_rootPath))
        {
            AddError(nameof(RootPath), "Путь до файла не может быть пустым");
            return;
        }

        if (!Path.Exists(_rootPath))
        {
            AddError(nameof(RootPath), "Такого пути не существует");
            return;
        }

        if (!Directory.Exists(_rootPath))
        {
            AddError(nameof(RootPath), "Путь должен указывать на папку");
        }
    }

    public void ValidateIncludePattern()
    {
        ClearErrors(nameof(IncludePattern));
        
        if (string.IsNullOrWhiteSpace(_includePattern))
        {
            AddError(nameof(IncludePattern), "Патерн не может быть пустым");
        }
    }

    public void ValidateExcludePattern()
    {
        ClearErrors(nameof(ExcludePattern));
        
        if (string.IsNullOrWhiteSpace(_excludePattern))
        {
            AddError(nameof(ExcludePattern), "Патерн не может быть пустым");
        }
    }

    public bool HasErrors => _errors.Any(kv => kv.Value.Count > 0);

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName == null || !_errors.TryGetValue(propertyName, out var errors))
            return Array.Empty<string>();

        return errors;
    }

    private void AddError(string propertyName, string errorMessage)
    {
        if (!_errors.TryGetValue(propertyName, out var value))
        {
            value = new List<string>();
            _errors[propertyName] = value;
        }

        if (value.Contains(errorMessage)) return;
        value.Add(errorMessage);
        OnErrorsChanged(propertyName);
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}