using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Color = Xceed.Drawing.Color;

namespace App.Domain.Utils;

public static class WordGenerator
{
    public static async Task GenerateReport(string basePath, ICollection<string> filePaths, string outputFilePath)
    {
        using var doc = DocX.Create(outputFilePath);

        doc.MarginTop = 56.66f;
        doc.MarginLeft = 85f;
        doc.MarginRight = 28.33f;
        doc.MarginBottom = 56.66f;

        foreach (var filePath in filePaths)
        {
            var pathPara = doc.InsertParagraph(Path.GetRelativePath(basePath, filePath) + ":")
                .SpacingBefore(12);
            pathPara.Font("Times New Roman").FontSize(14);
            pathPara.SpacingLine(18);

            var content = await File.ReadAllTextAsync(filePath);
            content = RemoveInvalidXmlChars(content);

            var borderTable = doc.AddTable(1, 1); 
            var cellPara = borderTable.Rows[0].Cells[0].Paragraphs.First();

            borderTable.SetBorder(TableBorderType.Top, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
            borderTable.SetBorder(TableBorderType.Bottom, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
            borderTable.SetBorder(TableBorderType.Left, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
            borderTable.SetBorder(TableBorderType.Right, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));

            cellPara.Append(content).Font("Cascadia Mono").FontSize(10);
            
            doc.InsertTable(borderTable);
        }

        doc.Save();
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