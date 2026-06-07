namespace SufiChain.Chat;

[Flags]
public enum ChatAttachmentAllowedFileTypes
{
    None = 0,
    Image = 1,
    Video = 2,
    Document = 4,
    Audio = 8,
    All = Image | Video | Document | Audio
}
