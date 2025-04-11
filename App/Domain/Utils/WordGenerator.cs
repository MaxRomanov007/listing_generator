using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private const float Inch = 2.54f;
    private const byte PointsInInch = 72;
    private const string DefaultFont = "Times New Roman";
    
    public static async Task GenerateReport(string basePath, ICollection<string> filePaths, string outputFilePath)
    {
        var cfg = AppSettings.Config.Generating;
        using var doc = DocX.Create(outputFilePath);

        doc.MarginTop = CmToPt(cfg.MarginTop);
        doc.MarginLeft = CmToPt(cfg.MarginLeft);
        doc.MarginRight = CmToPt(cfg.MarginRight);
        doc.MarginBottom = CmToPt(cfg.MarginBottom);
        
        doc.SetDefaultFont(new Font(DefaultFont));

        foreach (var filePath in filePaths)
        {
            var pathPara = doc.InsertParagraph(Path.GetRelativePath(basePath, filePath) + ":");
            pathPara.Font(cfg.MainFont).FontSize(cfg.MainFontSize);
            pathPara.SpacingLine(12 * cfg.MainLineSpacingMultiplier);
            pathPara.IndentationBefore = CmToPt(cfg.MainIndent);
            if (cfg.IsIntervalBeforeTitle)
            {
                pathPara.SpacingBefore(12);
            }
            if (cfg.IsIntervalAfterTitle)
            {
                pathPara.SpacingAfter(12);
            }

            var content = await File.ReadAllTextAsync(filePath);
            content = RemoveInvalidXmlChars(content);

            if (cfg.IsCodeInTable)
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
                cellPara.IndentationBefore = CmToPt(cfg.CodeIndent);

                doc.InsertTable(borderTable);
            }
            else
            {
                var codePara = doc.InsertParagraph(content)
                    .Font(cfg.CodeFont)
                    .FontSize(cfg.CodeFontSize)
                    .SpacingLine(12 * cfg.CodeLineSpacingMultiplier);
                codePara.IndentationBefore = CmToPt(cfg.CodeIndent);
            }
        }

        doc.Save();
        
        if (cfg.IsOpenAfterSave)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open saved file: {ex.Message}");
            }
        }
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

    private static float CmToPt(float cm)
    {
        return cm / Inch * PointsInInch;
    } 
}