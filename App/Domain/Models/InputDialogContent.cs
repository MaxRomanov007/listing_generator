using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace App.Domain.Models;

public class InputDialogContent: INotifyPropertyChanged, INotifyDataErrorInfo
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private readonly Dictionary<string, List<string>> _errors = new();

    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value;
            OnPropertyChanged();
            ClearErrors(nameof(Text));
        }
    }

    public void ValidateText()
    {
        ClearErrors(nameof(Text));
        
        if (string.IsNullOrWhiteSpace(_text))
        {
            AddError(nameof(Text), "Патерн не может быть пустым");
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