using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.FileManager.Uploads;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

/// <summary>sufi-chain#479 QA follow-ups on 8.4.6: invisible uploads, stuck rows, English rejection, fa digits.</summary>
public class FileUploadFollowUpTests
{
    private static readonly CultureInfo Fa = CultureInfo.GetCultureInfo("fa-IR");
    private static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");

    [Fact]
    public void Uploader_follows_the_structure_of_the_folder_it_is_opened_in()
    {
        var state = new UploaderStructureKeyState();

        // The File Manager first renders at the root, where it has no structure yet.
        state.ApplyParameter(null);
        state.Selected.ShouldBe(FileStructureKeys.General);

        // Then it moves into «پیوست‌های هوش مصنوعی»: later uploads must carry that structure, not General.
        state.ApplyParameter("AI").ShouldBeTrue();
        state.Selected.ShouldBe("AI");

        state.ApplyParameter("Chat.Attachments").ShouldBeTrue();
        state.Selected.ShouldBe("Chat.Attachments");
    }

    [Fact]
    public void Uploader_keeps_a_structure_the_user_picked_while_the_parameter_is_unchanged()
    {
        var state = new UploaderStructureKeyState();
        state.ApplyParameter(FileStructureKeys.General);

        state.Select("cms-media");
        state.ApplyParameter(FileStructureKeys.General).ShouldBeFalse();

        state.Selected.ShouldBe("cms-media");
    }

    [Fact]
    public void Results_land_on_their_own_row_after_earlier_rows_were_removed()
    {
        var tracker = new UploadRowTracker<Row>();
        var first = tracker.AddBatch(new[] { new Row("a.txt"), new Row("b.png") });
        tracker.Remove(tracker.Find(first)!); // a.txt finished and left the list

        var zipId = tracker.AddBatch(new[] { new Row("archive.zip") });

        // The browser reports the zip as the first file of its batch; by position it used to hit b.png.
        tracker.Find(zipId)!.Name.ShouldBe("archive.zip");
        tracker.Find(first + 1)!.Name.ShouldBe("b.png");
        tracker.Find(first).ShouldBeNull();
        tracker.Rows.Select(row => row.Name).ShouldBe(new[] { "b.png", "archive.zip" });
    }

    [Fact]
    public void Every_file_of_a_batch_gets_its_result_even_when_rows_leave_mid_batch()
    {
        var tracker = new UploadRowTracker<Row>();
        var first = tracker.AddBatch(new[] { new Row("1"), new Row("2"), new Row("3") });

        tracker.Remove(tracker.Find(first)!);
        tracker.Remove(tracker.Find(first + 1)!);

        // By position, index 2 was out of range once two rows had left, so file 3 stayed on «در حال پردازش…».
        tracker.Find(first + 2)!.Name.ShouldBe("3");
    }

    [Theory]
    [InlineData("AI", "AI", true)]
    [InlineData("AI", "ai", true)]
    [InlineData(null, "General", true)]
    [InlineData("AI", null, true)]
    [InlineData("AI", "General", false)]
    [InlineData("Chat.Attachments", "General", false)]
    public void Upload_must_match_the_structure_of_its_target_folder(string? folderKey, string? uploadKey, bool accepted)
    {
        UploadFolderStructureRule.Accepts(folderKey, uploadKey).ShouldBe(accepted);
    }

    [Fact]
    public void Size_limit_reads_in_Persian_digits_and_units_in_fa()
    {
        var fa = LoadTexts("fa");

        FileSizeText.Format(100L * 1024 * 1024, (key, number) => string.Format(fa[key], number), Fa)
            .ShouldBe("۱۰۰ مگابایت");
        FileSizeText.Format(1536, (key, number) => string.Format(fa[key], number), Fa)
            .ShouldBe("۱.۵ کیلوبایت");
        FileSizeText.Format(100L * 1024 * 1024, (key, number) => string.Format(LoadTexts("en")[key], number), En)
            .ShouldBe("100 MB");
    }

    [Fact]
    public void Digits_change_only_in_fa()
    {
        FileManagerDigits.Localize("10 MB", Fa).ShouldBe("۱۰ MB");
        FileManagerDigits.Localize("10 MB", En).ShouldBe("10 MB");
        FileManagerDigits.Localize("10", CultureInfo.GetCultureInfo("ar")).ShouldBe("10");
    }

    [Fact]
    public void Zip_rejection_is_Persian_in_the_fa_UI()
    {
        var result = UploadValidationResult.Invalid("File type 'application/zip' is not allowed", "FileTypeNotAllowed", "application/zip");

        UploadValidationMessages.Localize(new JsonLocalizer(LoadTexts("fa")), result, Fa)
            .ShouldBe("نوع فایل «application/zip» مجاز نیست");
    }

    [Fact]
    public void Size_rejection_uses_Persian_digits_in_fa()
    {
        var result = UploadValidationResult.Invalid("File size exceeds maximum allowed size of 100MB", "FileSizeExceedsMaximum", 100L);

        UploadValidationMessages.Localize(new JsonLocalizer(LoadTexts("fa")), result, Fa)!.ShouldContain("۱۰۰");
    }

    [Fact]
    public void Rejection_without_a_key_or_translation_falls_back_to_the_English_reason()
    {
        var noKey = new UploadValidationResult { IsValid = false, ErrorMessage = "Structure 'x' not found" };
        var unknownKey = UploadValidationResult.Invalid("English reason", "NoSuchKey");
        var localizer = new JsonLocalizer(LoadTexts("fa"));

        UploadValidationMessages.Localize(localizer, noKey, Fa).ShouldBe("Structure 'x' not found");
        UploadValidationMessages.Localize(localizer, unknownKey, Fa).ShouldBe("English reason");
    }

    [Theory]
    [InlineData("en")]
    [InlineData("fa")]
    [InlineData("ar")]
    [InlineData("es")]
    public void New_upload_texts_exist_in_every_language(string culture)
    {
        var texts = LoadTexts(culture);
        foreach (var key in new[]
                 {
                     "FileTypeNotAllowed", "FileExtensionNotAllowed", "FileExtensionBlocked", "FileNameNotAllowed",
                     "UploadFolderStructureMismatch", "FileSize:B", "FileSize:KB", "FileSize:MB", "FileSize:GB",
                     "FileSizeExceedsMaximum", "FolderPath"
                 })
        {
            texts.ShouldContainKey(key, $"{culture}.json misses {key}");
            texts[key].ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void Persian_upload_texts_follow_the_terminology_rules()
    {
        foreach (var (key, value) in LoadTexts("fa"))
        {
            value.ShouldNotContain("گفتگو", customMessage: key);
            value.ShouldNotContain("پرامپت", customMessage: key);
        }
    }

    private sealed record Row(string Name);

    private static Dictionary<string, string> LoadTexts(string culture)
    {
        var relative = Path.Combine("modules", "file-manager", "src", "SufiChain.SufiPlatform.FileManager.Domain.Shared",
            "Localization", "FileManager", culture + ".json");
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, relative)))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull($"{relative} not found above {AppContext.BaseDirectory}");
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(dir!.FullName, relative)));
        return document.RootElement.GetProperty("texts").EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.GetString() ?? string.Empty);
    }

    private sealed class JsonLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, string> _texts;

        public JsonLocalizer(Dictionary<string, string> texts) => _texts = texts;

        public LocalizedString this[string name] =>
            _texts.TryGetValue(name, out var value)
                ? new LocalizedString(name, value)
                : new LocalizedString(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments] =>
            _texts.TryGetValue(name, out var value)
                ? new LocalizedString(name, string.Format(value, arguments))
                : new LocalizedString(name, name, resourceNotFound: true);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _texts.Select(text => new LocalizedString(text.Key, text.Value));
    }
}
