using System.Collections.Generic;
using App.Domain.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace App.Pages;

public partial class MainPage : UserControl
{
    private MainPageFields _fields = new();
    private List<Pattern> _includePatterns = [];
    private List<Pattern> _excludePatterns = [];

    public MainPage()
    {
        InitializeComponent();
        DataContext = _fields;
        IncludePatternsItemsControl.ItemsSource = _includePatterns;
        ExcludePatternsItemsControl.ItemsSource = _excludePatterns;
    }

    private void IncludePatternAddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _fields.ValidateIncludePattern();
        if (_fields.HasErrors)
        {
            return;
        }
        
        _includePatterns.Add(new Pattern
        {
            Text = _fields.IncludePattern,
            Index = _includePatterns.Count
        });
        IncludePatternsItemsControl.ItemsSource = _includePatterns;
        
        _fields.IncludePattern = string.Empty;
    }
}