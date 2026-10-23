using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace HueTour.UseCases.Reports;

internal static class ExcelWorkbookWriter
{
    private static readonly XNamespace SpreadsheetNamespace =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelationshipNamespace =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PackageRelationshipNamespace =
        "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace ContentTypeNamespace =
        "http://schemas.openxmlformats.org/package/2006/content-types";
    private static readonly XNamespace XmlNamespace = XNamespace.Xml;

    public static byte[] Create(string sheetName, IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddXml(archive, "[Content_Types].xml", CreateContentTypes());
            AddXml(archive, "_rels/.rels", CreatePackageRelationships());
            AddXml(archive, "xl/workbook.xml", CreateWorkbook(sheetName));
            AddXml(archive, "xl/_rels/workbook.xml.rels", CreateWorkbookRelationships());
            AddXml(archive, "xl/worksheets/sheet1.xml", CreateWorksheet(rows));
        }

        return output.ToArray();
    }

    private static XDocument CreateContentTypes() => new(
        new XElement(ContentTypeNamespace + "Types",
            new XElement(ContentTypeNamespace + "Default",
                new XAttribute("Extension", "rels"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
            new XElement(ContentTypeNamespace + "Default",
                new XAttribute("Extension", "xml"),
                new XAttribute("ContentType", "application/xml")),
            new XElement(ContentTypeNamespace + "Override",
                new XAttribute("PartName", "/xl/workbook.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
            new XElement(ContentTypeNamespace + "Override",
                new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"))));

    private static XDocument CreatePackageRelationships() => new(
        new XElement(PackageRelationshipNamespace + "Relationships",
            new XElement(PackageRelationshipNamespace + "Relationship",
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                new XAttribute("Target", "xl/workbook.xml"))));

    private static XDocument CreateWorkbook(string sheetName) => new(
        new XElement(SpreadsheetNamespace + "workbook",
            new XAttribute(XNamespace.Xmlns + "r", RelationshipNamespace),
            new XElement(SpreadsheetNamespace + "sheets",
                new XElement(SpreadsheetNamespace + "sheet",
                    new XAttribute("name", sheetName),
                    new XAttribute("sheetId", 1),
                    new XAttribute(RelationshipNamespace + "id", "rId1")))));

    private static XDocument CreateWorkbookRelationships() => new(
        new XElement(PackageRelationshipNamespace + "Relationships",
            new XElement(PackageRelationshipNamespace + "Relationship",
                new XAttribute("Id", "rId1"),
                new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                new XAttribute("Target", "worksheets/sheet1.xml"))));

    private static XDocument CreateWorksheet(IReadOnlyList<IReadOnlyList<object?>> rows) => new(
        new XElement(SpreadsheetNamespace + "worksheet",
            new XElement(SpreadsheetNamespace + "sheetData",
                rows.Select((row, rowIndex) =>
                    new XElement(SpreadsheetNamespace + "row",
                        new XAttribute("r", rowIndex + 1),
                        row.Select((value, columnIndex) => CreateCell(value, columnIndex + 1, rowIndex + 1)))))));

    private static XElement CreateCell(object? value, int columnIndex, int rowIndex)
    {
        var cell = new XElement(SpreadsheetNamespace + "c",
            new XAttribute("r", string.Concat(GetColumnName(columnIndex), rowIndex.ToString(CultureInfo.InvariantCulture))));

        if (value is null)
        {
            cell.Add(new XAttribute("t", "inlineStr"),
                new XElement(SpreadsheetNamespace + "is", new XElement(SpreadsheetNamespace + "t")));
            return cell;
        }

        if (value is decimal or int or long or double or float)
        {
            cell.Add(new XElement(SpreadsheetNamespace + "v", Convert.ToString(value, CultureInfo.InvariantCulture)));
            return cell;
        }

        cell.Add(new XAttribute("t", "inlineStr"),
            new XElement(SpreadsheetNamespace + "is",
                new XElement(SpreadsheetNamespace + "t",
                    new XAttribute(XmlNamespace + "space", "preserve"),
                    Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)));
        return cell;
    }

    private static string GetColumnName(int columnIndex)
    {
        var name = new StringBuilder();
        while (columnIndex > 0)
        {
            columnIndex--;
            name.Insert(0, (char)('A' + columnIndex % 26));
            columnIndex /= 26;
        }

        return name.ToString();
    }

    private static void AddXml(ZipArchive archive, string path, XDocument document)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        document.Save(writer, SaveOptions.DisableFormatting);
    }
}
