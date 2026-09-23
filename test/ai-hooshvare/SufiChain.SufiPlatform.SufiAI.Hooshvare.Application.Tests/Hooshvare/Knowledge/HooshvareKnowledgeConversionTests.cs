using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare.Knowledge.Conversion;
using Volo.Abp;
using Xunit;
using Sheets = DocumentFormat.OpenXml.Spreadsheet;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare.Knowledge;

public class HooshvareKnowledgeConversionTests
{
    private const string WorkspaceName = "hooshvare-workspace";

    [Theory]
    [InlineData("كتاب", PdfPageTextDecision.Render)]
    [InlineData("باتک ینیشام", PdfPageTextDecision.Render)]
    [InlineData("Broken \uFFFD glyphs in this line of extracted text", PdfPageTextDecision.Render)]
    [InlineData("Private \uE012 use glyphs in this line of extracted text", PdfPageTextDecision.Render)]
    [InlineData("   ", PdfPageTextDecision.Render)]
    [InlineData("... --- ... *** ### @@@ ~~~ ::: ;;; ,,, !!! ??? ((( )))", PdfPageTextDecision.Render)]
    [InlineData("The quarterly report covers revenue, costs, and the hiring plan.", PdfPageTextDecision.Accept)]
    public void Quality_Gate_Should_Render_Arabic_Script_Garbled_And_Empty_Pages(string text, PdfPageTextDecision expected)
    {
        PdfPageTextQualityGate.Classify(text).ShouldBe(expected);
    }

    [Fact]
    public async Task Converter_Should_Reject_Legacy_Word_And_Unknown_Types()
    {
        var converter = CreateConverter();

        var legacy = await Should.ThrowAsync<BusinessException>(() => converter.ConvertAsync(Request("old.doc", "application/msword", [1])));
        legacy.Code.ShouldBe(AIHooshvareErrorCodes.KnowledgeLegacyWordNotSupported);

        var unknown = await Should.ThrowAsync<BusinessException>(() => converter.ConvertAsync(Request("deck.pptx", "application/vnd.ms-powerpoint", [1])));
        unknown.Code.ShouldBe(AIHooshvareErrorCodes.KnowledgeFileTypeNotSupported);
        unknown.Data["Extension"].ShouldBe(".pptx");
    }

    [Fact]
    public async Task Converter_Should_Route_Text_And_Normalize()
    {
        var converter = CreateConverter();

        var markdown = await converter.ConvertAsync(Request("notes.txt", "text/plain", new UTF8Encoding(false).GetBytes("علي\r\n\r\n\r\nكتاب")));

        markdown.ShouldBe("علی\n\nکتاب");
    }

    [Fact]
    public async Task Image_Without_Vision_Model_Should_Fail_With_Localized_Code()
    {
        var converter = CreateConverter(visionAvailable: false);

        var exception = await Should.ThrowAsync<BusinessException>(() => converter.ConvertAsync(Request("scan.webp", "image/webp", [1, 2, 3])));

        exception.Code.ShouldBe(AIHooshvareErrorCodes.KnowledgeConversionRequiresVision);
    }

    [Fact]
    public async Task Image_Should_Be_Transcribed_Through_The_Workspace_Vision_Model()
    {
        var ai = Substitute.For<IAIService>();
        ai.AnalyzeImageAsync(Arg.Any<VisionAnalysisRequest>(), Arg.Any<CancellationToken>())
            .Returns(new VisionAnalysisResponse
            {
                Description = "```markdown\n# رسید\nمبلغ: ۱۲۰\n```",
                InputTokens = 900,
                OutputTokens = 40
            });
        var converter = CreateConverter(visionAvailable: true, ai: ai);
        var request = Request("receipt.jpg", "image/jpeg", [1, 2, 3]);

        var markdown = await converter.ConvertAsync(request);

        markdown.ShouldBe("# رسید\nمبلغ: ۱۲۰");
        request.Usage.ModelCalls.ShouldBe(1);
        request.Usage.TotalTokens.ShouldBe(940);
        await ai.Received(1).AnalyzeImageAsync(
            Arg.Is<VisionAnalysisRequest>(request =>
                request.WorkspaceName == WorkspaceName
                && request.ImageFormat == "jpeg"
                && request.Prompt.Contains("Never translate")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Docx_Should_Produce_Headings_Lists_Tables_And_Links_In_Logical_Order()
    {
        var converter = CreateConverter(visionAvailable: false);

        var markdown = await converter.ConvertAsync(Request(
            "guide.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            CreateDocx()));

        markdown.ShouldContain("# راهنمای نصب");
        markdown.ShouldContain("- مرحله اول");
        markdown.ShouldContain("**مهم**");
        markdown.ShouldContain("| نام | مقدار |");
        markdown.ShouldContain("| نسخه | 2.0 |");
        markdown.ShouldContain("[سایت](https://example.com/)");
        markdown.IndexOf("راهنمای نصب", StringComparison.Ordinal)
            .ShouldBeLessThan(markdown.IndexOf("مرحله اول", StringComparison.Ordinal));
    }

    [Fact]
    public void Excel_Should_Repeat_Header_Every_Block_And_Cap_Rows()
    {
        var rows = new List<List<string>> { new() { "نام", "مقدار" } };
        for (var index = 1; index <= HooshvareExcelMarkdownHandler.MaxRowsPerSheet + 10; index++)
        {
            rows.Add(new List<string> { "ردیف " + index, index.ToString() });
        }

        var builder = new StringBuilder();
        HooshvareExcelMarkdownHandler.AppendSheet(builder, "فروش", rows, rows.Count);
        var markdown = builder.ToString();

        markdown.ShouldStartWith("## فروش");
        CountOccurrences(markdown, "| نام | مقدار |")
            .ShouldBe(HooshvareExcelMarkdownHandler.MaxRowsPerSheet / HooshvareExcelMarkdownHandler.RowsPerBlock);
        markdown.ShouldContain("| ردیف 5000 | 5000 |");
        markdown.ShouldNotContain("ردیف 5001");
        markdown.ShouldContain("<!-- truncated: 5000 of 5010 rows -->");
    }

    [Fact]
    public void Excel_Should_Fill_Merged_Cells_And_Trim_Empty_Rows_And_Columns()
    {
        var rows = new List<List<string>>
        {
            new() { "", "", "" },
            new() { "گروه", "", "مقدار" },
            new() { "الف", "", "1" },
            new() { "", "", "2" }
        };

        HooshvareExcelMarkdownHandler.ApplyMergedCells(rows, [new ExcelDataReader.CellRange(FromColumn: 0, FromRow: 2, ToColumn: 0, ToRow: 3)]);
        var trimmed = HooshvareExcelMarkdownHandler.TrimEmpty(rows);

        trimmed.Count.ShouldBe(3);
        trimmed.ShouldAllBe(row => row.Count == 2);
        trimmed[2][0].ShouldBe("الف");
    }

    [Fact]
    public async Task Excel_Should_Skip_Hidden_Sheets()
    {
        var converter = CreateConverter();

        var markdown = await converter.ConvertAsync(Request(
            "book.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            CreateXlsx()));

        markdown.ShouldContain("## Visible");
        markdown.ShouldContain("| Name | Qty |");
        markdown.ShouldContain("| Pen | 3 |");
        markdown.ShouldNotContain("Secret");
    }

    [Fact]
    public void Library_Consts_Should_Sanitize_Folder_Segment_And_Detect_Types()
    {
        var id = Guid.NewGuid();

        HooshvareKnowledgeLibraryConsts.GetFolderSegment("sales assistant/v2", id).ShouldBe("sales-assistant-v2");
        HooshvareKnowledgeLibraryConsts.GetFolderSegment(null, id).ShouldBe(id.ToString("N"));
        HooshvareKnowledgeLibraryConsts.IsSupported("Report.PDF").ShouldBeTrue();
        HooshvareKnowledgeLibraryConsts.IsSupported("old.doc").ShouldBeFalse();
        HooshvareKnowledgeLibraryConsts.IsMarkdown("notes.markdown").ShouldBeTrue();
    }

    private static HooshvareMarkdownConverter CreateConverter(bool visionAvailable = false, IAIService? ai = null)
    {
        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.FindAsync(WorkspaceName, Arg.Any<CancellationToken>()).Returns(new SufiAIWorkspaceDescriptor
        {
            Name = WorkspaceName,
            IsActive = true,
            IsReady = true,
            Capabilities = visionAvailable ? [SufiAICapability.Chat, SufiAICapability.Vision] : [SufiAICapability.Chat]
        });

        var vision = new HooshvareVisionTranscriber(ai ?? Substitute.For<IAIService>(), catalog);
        var options = Options.Create(new HooshvareKnowledgeOptions());
        IHooshvareFileToMarkdownHandler[] handlers =
        [
            new HooshvareTextMarkdownHandler(),
            new HooshvareDocxMarkdownHandler(vision, options, NullLogger<HooshvareDocxMarkdownHandler>.Instance),
            new HooshvareExcelMarkdownHandler(),
            new HooshvarePdfMarkdownHandler(vision, Substitute.For<ISufiAIChatService>(), options, NullLogger<HooshvarePdfMarkdownHandler>.Instance),
            new HooshvareImageMarkdownHandler(vision)
        ];

        return new HooshvareMarkdownConverter(handlers, NullLogger<HooshvareMarkdownConverter>.Instance);
    }

    private static HooshvareMarkdownConversionRequest Request(string fileName, string mimeType, byte[] content) => new()
    {
        FileName = fileName,
        MimeType = mimeType,
        Content = content,
        WorkspaceName = WorkspaceName
    };

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = text.IndexOf(value, StringComparison.Ordinal); index >= 0; index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }

    private static byte[] CreateDocx()
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = document.AddMainDocumentPart();
            var numberingPart = mainPart.AddNewPart<NumberingDefinitionsPart>();
            numberingPart.Numbering = new Numbering(
                new AbstractNum(
                    new Level(new NumberingFormat { Val = NumberFormatValues.Bullet }) { LevelIndex = 0 })
                { AbstractNumberId = 1 },
                new NumberingInstance(new AbstractNumId { Val = 1 }) { NumberID = 1 });

            var link = mainPart.AddHyperlinkRelationship(new Uri("https://example.com/"), true);
            mainPart.Document = new Document(new Body(
                new Paragraph(
                    new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }, new BiDi()),
                    new Run(new Text("راهنمای نصب"))),
                new Paragraph(
                    new ParagraphProperties(new NumberingProperties(new NumberingLevelReference { Val = 0 }, new NumberingId { Val = 1 })),
                    new Run(new Text("مرحله اول"))),
                new Paragraph(
                    new Run(new RunProperties(new Bold()), new Text("مهم")),
                    new Run(new Text(" ادامه متن") { Space = SpaceProcessingModeValues.Preserve }),
                    new Hyperlink(new Run(new Text("سایت"))) { Id = link.Id }),
                new Table(
                    new TableRow(Cell("نام"), Cell("مقدار")),
                    new TableRow(Cell("نسخه"), Cell("2.0")))));
        }

        return stream.ToArray();
    }

    private static TableCell Cell(string text) => new(new Paragraph(new Run(new Text(text))));

    private static byte[] CreateXlsx()
    {
        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Sheets.Workbook();
            var sheets = workbookPart.Workbook.AppendChild(new Sheets.Sheets());

            AddSheet(workbookPart, sheets, 1, "Visible", null, [["Name", "Qty"], ["Pen", "3"]]);
            AddSheet(workbookPart, sheets, 2, "Hidden", Sheets.SheetStateValues.Hidden, [["Secret", "1"]]);
            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    private static void AddSheet(
        WorkbookPart workbookPart,
        Sheets.Sheets sheets,
        uint sheetId,
        string name,
        Sheets.SheetStateValues? state,
        string[][] values)
    {
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        var data = new Sheets.SheetData();
        for (var rowIndex = 0; rowIndex < values.Length; rowIndex++)
        {
            var row = new Sheets.Row { RowIndex = (uint)(rowIndex + 1) };
            for (var columnIndex = 0; columnIndex < values[rowIndex].Length; columnIndex++)
            {
                row.AppendChild(new Sheets.Cell
                {
                    CellReference = (char)('A' + columnIndex) + (rowIndex + 1).ToString(),
                    DataType = Sheets.CellValues.InlineString,
                    InlineString = new Sheets.InlineString(new Sheets.Text(values[rowIndex][columnIndex]))
                });
            }

            data.AppendChild(row);
        }

        worksheetPart.Worksheet = new Sheets.Worksheet(data);
        var sheet = new Sheets.Sheet { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetId, Name = name };
        if (state.HasValue)
        {
            sheet.State = state.Value;
        }

        sheets.AppendChild(sheet);
    }
}
