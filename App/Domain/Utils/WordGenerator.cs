using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Domain.Models;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Color = Xceed.Drawing.Color;

namespace App.Domain.Utils;

public static class WordGenerator
{
    public static async Task GenerateReport(string basePath, ICollection<string> filePaths, string outputFilePath)
    {
        var cfg = AppSettings.Config.Generating;
        using var doc = DocX.Create(outputFilePath);

        doc.MarginTop = DocXHelper.CmToPt(cfg.MarginTop);
        doc.MarginLeft = DocXHelper.CmToPt(cfg.MarginLeft);
        doc.MarginRight = DocXHelper.CmToPt(cfg.MarginRight);
        doc.MarginBottom = DocXHelper.CmToPt(cfg.MarginBottom);

        if (cfg.IsTreeGenerating)
        {
            var paths = filePaths.Select(p => Path.GetRelativePath(basePath, p)).ToArray();
            AddTree(cfg, doc, paths, Path.GetFileName(basePath));
        }

        foreach (var filePath in filePaths)
        {
            AddTitleParagraph(cfg, doc, GetPath(cfg, basePath, filePath) + ":");

            var content = await File.ReadAllTextAsync(filePath);
            content = RemoveInvalidXmlChars(content);

            if (cfg.IsCodeInTable)
            {
                AddCodeInTableParagraph(cfg, doc, content);
            }
            else
            {
                AddCodeParagraph(cfg, doc, content);
            }
        }

        doc.Save();
        
        if (cfg.IsOpenAfterSave)
        {
            Files.TryStart(outputFilePath);
        }
    }

    private static string GetPath(GeneratingConfig cfg, string basePath, string fullPath)
    {
        return cfg.IsFilesWithPath ? Path.GetRelativePath(basePath, fullPath) : Path.GetFileName(fullPath);
    }

    private static void AddTree(GeneratingConfig cfg, DocX doc, ICollection<string> paths, string origin)
    {
        AddTitleParagraph(cfg, doc, "Структура проекта:");
        var tree = TreeBuilder.BuildTree(paths, origin);
        if (cfg.IsCodeInTable)
        {
            AddCodeInTableParagraph(cfg, doc, tree);
        }
        else
        {
            AddCodeParagraph(cfg, doc, tree);
        }
    }

    private static void AddTitleParagraph(GeneratingConfig cfg, DocX doc, string text)
    {
        var pathPara = doc.InsertParagraph(text);
        pathPara.Font(cfg.MainFont).FontSize(cfg.MainFontSize);
        pathPara.SpacingLine(12 * cfg.MainLineSpacingMultiplier);
        pathPara.IndentationBefore = DocXHelper.CmToPt(cfg.MainIndent);
        if (cfg.IsIntervalBeforeTitle)
        {
            pathPara.SpacingBefore(12);
        }
        if (cfg.IsIntervalAfterTitle)
        {
            pathPara.SpacingAfter(12);
        }
    }

    private static void AddCodeInTableParagraph(GeneratingConfig cfg, DocX doc, string content)
    {
        var borderTable = doc.AddTable(1, 1);
        var cellPara = borderTable.Rows[0].Cells[0].Paragraphs.First();

        borderTable.SetBorder(TableBorderType.Top,
            new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
        borderTable.SetBorder(TableBorderType.Bottom,
            new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
        borderTable.SetBorder(TableBorderType.Left,
            new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
        borderTable.SetBorder(TableBorderType.Right,
            new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));

        cellPara
            .Append(content)
            .Font(cfg.CodeFont)
            .FontSize(cfg.CodeFontSize)
            .SpacingLine(12 * cfg.CodeLineSpacingMultiplier);
        cellPara.IndentationBefore = DocXHelper.CmToPt(cfg.CodeIndent);

        doc.InsertTable(borderTable);
    }

    private static void AddCodeParagraph(GeneratingConfig cfg, DocX doc, string content)
    {
        var codePara = doc.InsertParagraph(content)
            .Font(cfg.CodeFont)
            .FontSize(cfg.CodeFontSize)
            .SpacingLine(12 * cfg.CodeLineSpacingMultiplier);
        codePara.IndentationBefore = DocXHelper.CmToPt(cfg.CodeIndent);
    }
    
    private static string RemoveInvalidXmlChars(string text)
    {
        if (string.IsNullOrEmpty(text)) 
            return text;

        var validXmlChars = text.Where(ch => 
            ch == 0x9 || ch == 0xA || ch == 0xD || 
            (ch >= 0x20 && ch <= 0xD7FF) || 
            (ch >= 0xE000 && ch <= 0xFFFD)).ToArray();

        return new string(validXmlChars);
    }
}