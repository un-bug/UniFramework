using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

internal static class XlsxWorkbookReader
{
    public static List<XlsxSheetData> Read(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var sharedStrings = ReadSharedStrings(archive);
        var relationships = ReadWorkbookRelationships(archive);
        var sheets = ReadWorkbookSheets(archive);
        var result = new List<XlsxSheetData>();

        foreach (var sheet in sheets)
        {
            if (!relationships.TryGetValue(sheet.RelationshipId, out var target))
            {
                continue;
            }

            string entryName = NormalizePartPath(target.StartsWith("/") ? target.Substring(1) : "xl/" + target);
            var entry = archive.GetEntry(entryName);
            if (entry == null)
            {
                continue;
            }

            result.Add(ReadSheet(entry, sheet.Name, sharedStrings));
        }

        return result;
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        var strings = new List<string>();
        if (entry == null)
        {
            return strings;
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace ns = document.Root.Name.Namespace;

        foreach (var item in document.Root.Elements(ns + "si"))
        {
            var text = string.Concat(item.Descendants(ns + "t").Select(element => element.Value));
            strings.Add(text);
        }

        return strings;
    }

    private static Dictionary<string, string> ReadWorkbookRelationships(ZipArchive archive)
    {
        var relationships = new Dictionary<string, string>();
        var entry = archive.GetEntry("xl/_rels/workbook.xml.rels");
        if (entry == null)
        {
            return relationships;
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace ns = document.Root.Name.Namespace;

        foreach (var relationship in document.Root.Elements(ns + "Relationship"))
        {
            string id = (string)relationship.Attribute("Id");
            string target = (string)relationship.Attribute("Target");
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(target))
            {
                relationships[id] = target;
            }
        }

        return relationships;
    }

    private static List<XlsxSheetRef> ReadWorkbookSheets(ZipArchive archive)
    {
        var sheets = new List<XlsxSheetRef>();
        var entry = archive.GetEntry("xl/workbook.xml");
        if (entry == null)
        {
            return sheets;
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace ns = document.Root.Name.Namespace;
        XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        foreach (var sheet in document.Root.Descendants(ns + "sheet"))
        {
            string name = (string)sheet.Attribute("name");
            string relationshipId = (string)sheet.Attribute(relNs + "id");
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(relationshipId))
            {
                sheets.Add(new XlsxSheetRef(name, relationshipId));
            }
        }

        return sheets;
    }

    private static XlsxSheetData ReadSheet(ZipArchiveEntry entry, string sheetName, List<string> sharedStrings)
    {
        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace ns = document.Root.Name.Namespace;
        var sheet = new XlsxSheetData(sheetName);

        foreach (var row in document.Root.Descendants(ns + "row"))
        {
            foreach (var cell in row.Elements(ns + "c"))
            {
                string reference = (string)cell.Attribute("r");
                if (!TryParseCellReference(reference, out int rowIndex, out int columnIndex))
                {
                    continue;
                }

                sheet.SetCell(rowIndex, columnIndex, ReadCellValue(cell, ns, sharedStrings));
            }
        }

        return sheet;
    }

    private static string ReadCellValue(XElement cell, XNamespace ns, List<string> sharedStrings)
    {
        string xlsxValueType = (string)cell.Attribute("t");

        if (xlsxValueType == "inlineStr")
        {
            return string.Concat(cell.Descendants(ns + "t").Select(element => element.Value));
        }

        string rawValue = (string)cell.Element(ns + "v") ?? string.Empty;
        if (xlsxValueType == "s" && int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sharedStringIndex))
        {
            return sharedStringIndex >= 0 && sharedStringIndex < sharedStrings.Count ? sharedStrings[sharedStringIndex] : string.Empty;
        }

        if (xlsxValueType == "b")
        {
            return rawValue == "1" ? "TRUE" : "FALSE";
        }

        return rawValue;
    }

    private static bool TryParseCellReference(string reference, out int rowIndex, out int columnIndex)
    {
        rowIndex = -1;
        columnIndex = -1;
        if (string.IsNullOrEmpty(reference))
        {
            return false;
        }

        int index = 0;
        int column = 0;
        while (index < reference.Length && char.IsLetter(reference[index]))
        {
            column = column * 26 + (char.ToUpperInvariant(reference[index]) - 'A' + 1);
            index++;
        }

        if (column == 0 || index >= reference.Length)
        {
            return false;
        }

        if (!int.TryParse(reference.Substring(index), NumberStyles.Integer, CultureInfo.InvariantCulture, out int row))
        {
            return false;
        }

        rowIndex = row - 1;
        columnIndex = column - 1;
        return rowIndex >= 0 && columnIndex >= 0;
    }

    private static string NormalizePartPath(string path)
    {
        var parts = new List<string>();
        foreach (string part in path.Replace('\\', '/').Split('/'))
        {
            if (string.IsNullOrEmpty(part) || part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }

                continue;
            }

            parts.Add(part);
        }

        return string.Join("/", parts);
    }

    private readonly struct XlsxSheetRef
    {
        public readonly string Name;
        public readonly string RelationshipId;

        public XlsxSheetRef(string name, string relationshipId)
        {
            Name = name;
            RelationshipId = relationshipId;
        }
    }
}

internal sealed class XlsxSheetData
{
    private readonly Dictionary<int, Dictionary<int, string>> m_Cells = new Dictionary<int, Dictionary<int, string>>();

    public string Name { get; }

    public int LastRowIndex { get; private set; } = -1;

    public int LastColumnIndex { get; private set; } = -1;

    public XlsxSheetData(string name)
    {
        Name = name;
    }

    public void SetCell(int rowIndex, int columnIndex, string value)
    {
        if (!m_Cells.TryGetValue(rowIndex, out var row))
        {
            row = new Dictionary<int, string>();
            m_Cells.Add(rowIndex, row);
        }

        row[columnIndex] = value ?? string.Empty;
        if (rowIndex > LastRowIndex)
        {
            LastRowIndex = rowIndex;
        }

        if (columnIndex > LastColumnIndex)
        {
            LastColumnIndex = columnIndex;
        }
    }

    public string GetCell(int rowIndex, int columnIndex)
    {
        if (m_Cells.TryGetValue(rowIndex, out var row) && row.TryGetValue(columnIndex, out string value))
        {
            return value;
        }

        return string.Empty;
    }

    public bool HasRow(int rowIndex)
    {
        return m_Cells.ContainsKey(rowIndex);
    }
}
