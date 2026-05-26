using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using NPOI.HWPF;
using NPOI.HWPF.UserModel;
using UglyToad.PdfPig;

namespace DocxDiffTool;

public class DiffEntry
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("old")] public string? Old { get; set; }
    [JsonPropertyName("new")] public string? New { get; set; }
    [JsonPropertyName("old_spans")] public List<SpanEntry>? OldSpans { get; set; }
    [JsonPropertyName("new_spans")] public List<SpanEntry>? NewSpans { get; set; }
}

public class SpanEntry
{
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
}

public class DiffStats
{
    [JsonPropertyName("equal")] public int Equal { get; set; }
    [JsonPropertyName("deleted")] public int Deleted { get; set; }
    [JsonPropertyName("inserted")] public int Inserted { get; set; }
    [JsonPropertyName("replaced")] public int Replaced { get; set; }
}

public class CompareResult
{
    [JsonPropertyName("name1")] public string Name1 { get; set; } = "";
    [JsonPropertyName("name2")] public string Name2 { get; set; } = "";
    [JsonPropertyName("text1")] public string Text1 { get; set; } = "";
    [JsonPropertyName("text2")] public string Text2 { get; set; } = "";
    [JsonPropertyName("diffs")] public List<DiffEntry> Diffs { get; set; } = new();
    [JsonPropertyName("stats")] public DiffStats Stats { get; set; } = new();
}

public static class DiffService
{
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    public static List<string> ExtractText(Stream stream)
    {
        // Peek magic bytes to detect format
        byte[] header = new byte[8];
        int read = stream.Read(header, 0, header.Length);
        stream.Position = 0;

        if (read >= 4 && header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04)
            return ExtractTextFromDocx(stream);

        if (read >= 8 && header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0)
            return ExtractTextFromDoc(stream);

        // PDF: starts with "%PDF"
        if (read >= 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46)
            return ExtractTextFromPdf(stream);

        // Fallback: plain text (.txt / .md)
        return ExtractTextFromPlainText(stream);
    }

    private static List<string> ExtractTextFromDocx(Stream stream)
    {
        var paragraphs = new List<string>();
        ZipArchive zip;
        try { zip = new ZipArchive(stream, ZipArchiveMode.Read); }
        catch (InvalidDataException)
        {
            throw new InvalidDataException("文件格式不支持。请上传 .docx 格式的 Word 文档。\n\n当前支持格式：.docx / .doc / .txt / .md");
        }
        var entry = zip.GetEntry("word/document.xml")
            ?? throw new InvalidDataException("文件格式不支持。未在文件中找到 Word 文档内容。");
        using var xml = entry.Open();
        var doc = XDocument.Load(xml);
        foreach (var p in doc.Descendants(W + "p"))
        {
            var texts = p.Descendants(W + "t").Select(t => t.Value);
            var line = string.Concat(texts);
            if (!string.IsNullOrWhiteSpace(line))
                paragraphs.Add(line);
        }
        return paragraphs;
    }

    private static List<string> ExtractTextFromDoc(Stream stream)
    {
        var paragraphs = new List<string>();
        var hwpfDoc = new HWPFDocument(stream);
        var range = hwpfDoc.GetRange();
        for (int i = 0; i < range.NumParagraphs; i++)
        {
            var para = range.GetParagraph(i);
            var texts = new List<string>();
            for (int j = 0; j < para.NumCharacterRuns; j++)
                texts.Add(para.GetCharacterRun(j).Text);
            var line = string.Concat(texts);
            if (!string.IsNullOrWhiteSpace(line))
                paragraphs.Add(line);
        }
        return paragraphs;
    }

    private static List<string> ExtractTextFromPdf(Stream stream)
    {
        var paragraphs = new List<string>();
        using var pdf = PdfDocument.Open(stream);
        foreach (var page in pdf.GetPages())
        {
            var pageText = page.Text;
            if (!string.IsNullOrWhiteSpace(pageText))
            {
                var lines = pageText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        paragraphs.Add(trimmed);
                }
            }
        }
        return paragraphs;
    }

    private static List<string> ExtractTextFromPlainText(Stream stream)
    {
        var paragraphs = new List<string>();
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
                paragraphs.Add(line);
        }
        return paragraphs;
    }

    public static List<DiffEntry> BuildDiffs(List<string> a, List<string> b)
    {
        var sm = new SequenceMatcher<string>(a.ToArray(), b.ToArray());
        var raw = sm.GetOpcodes();
        var diffs = new List<DiffEntry>();
        int i = 0;

        while (i < raw.Count)
        {
            var (tag, i1, i2, j1, j2) = raw[i];
            switch (tag)
            {
                case "equal":
                    for (int k = i1; k < i2; k++)
                        diffs.Add(new DiffEntry { Type = "equal", Old = a[k], New = a[k] });
                    break;
                case "delete":
                    if (i + 1 < raw.Count && raw[i + 1].Tag == "insert")
                    {
                        var (_, ni1, ni2, nj1, nj2) = raw[i + 1];
                        var oldPars = a.GetRange(i1, i2 - i1);
                        var newPars = b.GetRange(j1, j2 - j1);
                        foreach (var d in AlignParagraphs(oldPars, newPars))
                            diffs.Add(d);
                        i++;
                    }
                    else
                    {
                        for (int k = i1; k < i2; k++)
                            diffs.Add(new DiffEntry { Type = "delete", Old = a[k] });
                    }
                    break;
                case "insert":
                    for (int k = j1; k < j2; k++)
                        diffs.Add(new DiffEntry { Type = "insert", New = b[k] });
                    break;
                case "replace":
                    var oldP = a.GetRange(i1, i2 - i1);
                    var newP = b.GetRange(j1, j2 - j1);
                    foreach (var d in AlignParagraphs(oldP, newP))
                        diffs.Add(d);
                    break;
            }
            i++;
        }

        return diffs;
    }

    private static List<DiffEntry> AlignParagraphs(List<string> oldPars, List<string> newPars)
    {
        var result = new List<DiffEntry>();
        var usedNew = new HashSet<int>();

        foreach (var op in oldPars)
        {
            string? best = null;
            double bestScore = 0;
            int bestIdx = -1;

            for (int j = 0; j < newPars.Count; j++)
            {
                if (usedNew.Contains(j)) continue;
                var score = new SequenceMatcher<char>(op.ToCharArray(), newPars[j].ToCharArray()).Ratio();
                if (score > bestScore) { bestScore = score; best = newPars[j]; bestIdx = j; }
            }

            if (best != null && bestScore > 0.35)
            {
                usedNew.Add(bestIdx);
                var (oldS, newS) = WordDiff(op, best);
                result.Add(new DiffEntry
                {
                    Type = "replace", Old = op, New = best,
                    OldSpans = oldS, NewSpans = newS
                });
            }
            else
            {
                result.Add(new DiffEntry { Type = "delete", Old = op });
            }
        }

        for (int j = 0; j < newPars.Count; j++)
            if (!usedNew.Contains(j))
                result.Add(new DiffEntry { Type = "insert", New = newPars[j] });

        return result;
    }

    private static (List<SpanEntry>, List<SpanEntry>) WordDiff(string old, string n)
    {
        var sm = new SequenceMatcher<char>(old.ToCharArray(), n.ToCharArray());
        var oldSpans = new List<SpanEntry>();
        var newSpans = new List<SpanEntry>();

        foreach (var (tag, i1, i2, j1, j2) in sm.GetOpcodes())
        {
            switch (tag)
            {
                case "equal":
                    var t = old[i1..i2];
                    oldSpans.Add(new SpanEntry { Text = t, Type = "equal" });
                    newSpans.Add(new SpanEntry { Text = t, Type = "equal" });
                    break;
                case "delete":
                    oldSpans.Add(new SpanEntry { Text = old[i1..i2], Type = "delete" });
                    break;
                case "insert":
                    newSpans.Add(new SpanEntry { Text = n[j1..j2], Type = "insert" });
                    break;
                case "replace":
                    oldSpans.Add(new SpanEntry { Text = old[i1..i2], Type = "delete" });
                    newSpans.Add(new SpanEntry { Text = n[j1..j2], Type = "insert" });
                    break;
            }
        }

        return (oldSpans, newSpans);
    }
}
