using SufiChain.SufiAbp.FileManager.Configuration;

namespace SufiChain.SufiAbp.FileManager.Data;

public static class FileManagerFileStructureSeedTexts
{
    public const string ResourceName = FileStructureLocalizationRegistry.DefaultResourceName;

    public const string GeneralKey = FileStructureKeys.General;

    public static IReadOnlyDictionary<string, string> GeneralDisplayName { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["fa"] = "فایل‌های عمومی",
        ["en"] = "General Files",
        ["ar"] = "الملفات العامة",
        ["es"] = "Archivos generales"
    };

    public static IReadOnlyDictionary<string, string> GeneralDescription { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["fa"] = "هر نوع فایل متداول: تصاویر، ویدیو، اسناد (PDF، Word، Excel، CSV، PowerPoint و ...) و صوتی. مناسب برای ذخیره‌سازی فایل عمومی.",
        ["en"] = "Upload any common file type: images, videos, documents (PDF, Word, Excel, CSV, PowerPoint, etc.), and audio. Suitable for general-purpose file storage.",
        ["ar"] = "رفع أي نوع ملف شائع: صور، فيديو، مستندات (PDF، Word، Excel، CSV، PowerPoint، إلخ)، وصوت. مناسب لتخزين الملفات العامة.",
        ["es"] = "Suba cualquier tipo de archivo común: imágenes, vídeos, documentos (PDF, Word, Excel, CSV, PowerPoint, etc.) y audio. Adecuado para almacenamiento de archivos de uso general."
    };
}
