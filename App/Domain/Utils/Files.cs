using System;
using System.Diagnostics;

namespace App.Domain.Utils;

public static class Files
{
    public static void TryStart(string filePath)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open file \"{filePath}\": {ex.Message}");
        }
    }
}