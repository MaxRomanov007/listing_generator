using System.Collections.Generic;

namespace App.Domain.Models;

public class Pattern
{
    public string Text { get; set; } = string.Empty;
    public int Index { get; set; }
}

public class Patterns
{
    public List<Pattern> Include { get; set; } = [];
    public List<Pattern> Exclude { get; set; } = [];
}