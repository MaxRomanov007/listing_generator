using System;
using System.IO;
using App.Domain.Static;
using App.Domain.Utils;
using App.Pages;
using Avalonia.Controls;

namespace App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = Content;
        Content.Content = new MainPage();
    }
}