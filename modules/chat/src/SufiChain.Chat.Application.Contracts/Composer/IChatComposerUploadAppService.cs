using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.FileManager.FileItems;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Composer;

public class ChatComposerUploadInput
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [StringLength(256)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string MimeType { get; set; } = string.Empty;

    [Required]
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public bool IsVoiceRecording { get; set; }
}

public interface IChatComposerUploadAppService : IApplicationService
{
    Task<FileItemDto> UploadAsync(ChatComposerUploadInput input);
}
