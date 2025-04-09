using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace App.Domain.Utils;

public static class FileScanner
{
    public static List<string> ScanFiles(string rootDir, ICollection<string> includePatterns, ICollection<string> excludePatterns)
    {
        var matcher = new Matcher();

        if (includePatterns.Count > 0)
        {
            matcher.AddIncludePatterns(includePatterns);
        }
        else
        {
            matcher.AddInclude("**/*");
        }

        if (excludePatterns.Count > 0)
        {
            matcher.AddExcludePatterns(excludePatterns);
        }

        var directory = new DirectoryInfoWrapper(new DirectoryInfo(rootDir));
        var result = matcher.Execute(directory);

        return result.Files.Select(f => Path.Combine(rootDir, f.Path)).ToList();
    }
}