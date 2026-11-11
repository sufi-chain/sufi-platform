using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileStructures;

public class FileStructureLifecycleTests : FileManagerApplicationTestBase<SufiFileManagerApplicationTestModule>
{
    private readonly IFileStructureAppService _structures;
    private readonly IFileManagerAppService _fileManager;
    private readonly IFileFolderRepository _folders;
    private readonly IFileItemRepository _files;
    private readonly IGuidGenerator _guids;

    public FileStructureLifecycleTests()
    {
        _structures = GetRequiredService<IFileStructureAppService>();
        _fileManager = GetRequiredService<IFileManagerAppService>();
        _folders = GetRequiredService<IFileFolderRepository>();
        _files = GetRequiredService<IFileItemRepository>();
        _guids = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Delete_removes_the_structure_root_and_its_files()
    {
        var created = await _structures.CreateAsync(Structure("QaLife01", "QA B59 1111"));
        var root = await WithUnitOfWorkAsync(() => _folders.FindByPathAsync("/QaLife01", null));
        root.ShouldNotBeNull();

        var fileId = _guids.Create();
        await WithUnitOfWorkAsync(() => _files.InsertAsync(new FileItem(
            fileId,
            tenantId: null,
            name: "note.txt",
            originalName: "note.txt",
            blobName: "qa-life-01.txt",
            mimeType: "text/plain",
            size: 4,
            fileType: FileType.Document,
            structureKey: "QaLife01")
        {
            FolderId = root.Id
        }, autoSave: true));

        await _structures.DeleteAsync(created.Id);

        (await WithUnitOfWorkAsync(() => _folders.FindByPathAsync("/QaLife01", null))).ShouldBeNull();
        (await WithUnitOfWorkAsync(() => _files.FindAsync(fileId))).ShouldBeNull();
        await Should.ThrowAsync<Exception>(() => _structures.GetAsync(created.Id));
    }

    [Fact]
    public async Task An_admin_can_delete_an_orphaned_structure_root()
    {
        var folderId = _guids.Create();
        await WithUnitOfWorkAsync(() => _folders.InsertAsync(new FileFolder(
            folderId,
            tenantId: null,
            name: "QA B59 1111",
            path: "/QaB591111",
            type: FolderType.Structure,
            structureKey: "QaB591111"), autoSave: true));

        var contents = await GetRequiredService<IFolderAppService>().GetContentsAsync(new GetFolderContentsInput());
        var orphan = contents.Folders.Single(folder => folder.Id == folderId);
        orphan.CanDelete.ShouldBeTrue();
        orphan.Name.ShouldBe("QA B59 1111");

        var result = await _fileManager.DeleteItemsAsync(new BulkDeleteInput
        {
            FolderIds = { folderId }
        });

        result.Success.ShouldBeTrue();
        result.FailedCount.ShouldBe(0);
        (await WithUnitOfWorkAsync(() => _folders.FindAsync(folderId))).ShouldBeNull();
    }

    [Fact]
    public async Task A_live_structure_root_stays_protected()
    {
        await _structures.CreateAsync(Structure("QaLive02", "Live"));
        var root = await WithUnitOfWorkAsync(() => _folders.FindByPathAsync("/QaLive02", null));
        root.ShouldNotBeNull();

        var contents = await GetRequiredService<IFolderAppService>().GetContentsAsync(new GetFolderContentsInput());
        contents.Folders.Single(folder => folder.Id == root.Id).CanDelete.ShouldBeFalse();

        var result = await _fileManager.DeleteItemsAsync(new BulkDeleteInput
        {
            FolderIds = { root.Id }
        });

        result.Success.ShouldBeFalse();
        result.FailedCount.ShouldBe(1);
        (await WithUnitOfWorkAsync(() => _folders.FindAsync(root.Id))).ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_rejects_a_duplicate_key()
    {
        var input = Structure("QaDup01", "Duplicate");
        await _structures.CreateAsync(input);

        var exception = await Should.ThrowAsync<BusinessException>(() => _structures.CreateAsync(input));
        exception.Code.ShouldBe(FileManagerErrorCodes.FileStructureAlreadyExists);
        exception.Data["Key"].ShouldBe("QaDup01");
    }

    private static CreateUpdateFileStructureDto Structure(string key, string displayName) => new()
    {
        Key = key,
        DisplayName = displayName,
        AllowedFileTypes = FileType.Document,
        AllowedExtensions = "txt",
        AllowedMimeTypes = "text/plain",
        MaxFileSize = 1024
    };
}

public class FileStructureKeyMessageTests
{
    [Theory]
    [InlineData("_qa")]
    [InlineData("")]
    public void A_key_that_starts_with_an_underscore_is_invalid(string key)
    {
        FileStructureKeyRules.IsFormatValid(key).ShouldBeFalse();
    }

    [Theory]
    [InlineData("Qa1")]
    [InlineData("A_b.1")]
    public void A_key_that_starts_with_a_letter_or_digit_is_valid(string key)
    {
        FileStructureKeyRules.IsFormatValid(key).ShouldBeTrue();
    }

    [Fact]
    public void The_format_error_code_is_the_localization_key()
    {
        var attribute = typeof(CreateUpdateFileStructureDto)
            .GetProperty(nameof(CreateUpdateFileStructureDto.Key))!
            .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RegularExpressionAttribute), false)
            .Cast<System.ComponentModel.DataAnnotations.RegularExpressionAttribute>()
            .Single();

        attribute.ErrorMessage.ShouldBe(FileStructureKeyRules.FormatInvalid);
    }

    [Theory]
    [InlineData("en", "Key must start with a letter or digit")]
    [InlineData("fa", "کلید باید با حرف یا رقم شروع شود")]
    [InlineData("ar", "يجب أن يبدأ المفتاح بحرف أو رقم")]
    [InlineData("es", "La clave debe comenzar con una letra o un dígito")]
    public void KeyFormatInvalid_maps_in_every_culture(string culture, string expected)
    {
        var json = File.ReadAllText(LocalizationFile(culture));
        json.ShouldContain($"\"{FileStructureKeyRules.FormatInvalid}\"");
        json.ShouldContain(expected);
        json.ShouldContain("\"StructureKeyAlreadyExists\"");
    }

    [Fact]
    public void The_wizard_localizes_the_format_error_and_checks_the_key_before_create()
    {
        var wizard = File.ReadAllText(RepositoryFile(
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Pages/FileStructureWizard.razor.cs"));

        wizard.ShouldContain("FileStructureKeyRules.FormatInvalid");
        wizard.ShouldContain("ExistsAsync(_editModel.Key)");
        wizard.ShouldContain("StructureKeyAlreadyExists");
    }

    [Fact]
    public void Asset_manager_lists_root_folders_and_deletes_only_when_the_server_allows_it()
    {
        var explorer = File.ReadAllText(RepositoryFile(
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Components/FileManager/FileManager.razor"));
        explorer.ShouldNotContain("firstRoot");
        explorer.ShouldContain("CanDeleteSelection");

        var toolbar = File.ReadAllText(RepositoryFile(
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Components/FileManager/FileManagerToolbar.razor"));
        toolbar.ShouldContain("CanDeleteSelection");
    }

    private static string LocalizationFile(string culture) => RepositoryFile(
        $"sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Domain.Shared/Localization/FileManager/{culture}.json");

    private static string RepositoryFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException(relativePath);
    }
}
