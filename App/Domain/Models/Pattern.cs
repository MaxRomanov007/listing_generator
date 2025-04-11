using System.Collections.Generic;

namespace App.Domain.Models;

public class Patterns
{
    public List<string> Include { get; set; } = [];
    public List<string> Exclude { get; set; } = [];
}