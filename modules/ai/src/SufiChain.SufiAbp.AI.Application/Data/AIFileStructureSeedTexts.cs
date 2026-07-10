namespace SufiChain.SufiAbp.AI.Data;

public static class AIFileStructureSeedTexts
{
    public const string ResourceName = "AI";

    public const string StructureKey = AIFileStructureKeys.AI;

    public static IReadOnlyDictionary<string, string> DisplayName { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["fa"] = "پیوست‌های هوشواره",
        ["en"] = "AI Management Files",
        ["ar"] = "ملفات إدارة الذكاء الاصطناعي",
        ["es"] = "Archivos de Gestión de IA"
    };

    public static IReadOnlyDictionary<string, string> Description { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["fa"] = "بارگذاری فایل‌ها برای فضاهای کاری هوشواره: تصاویر، صدا، ویدیو و اسناد. پشتیبانی از تمام انواع فایل‌های رایج مرتبط با هوشواره با تولید خودکار تصاویر کوچک و تبدیل WebP.",
        ["en"] = "Upload files for AI workspaces: images, audio, video, and documents. Supports all common AI-related file types with automatic thumbnail generation and WebP conversion.",
        ["ar"] = "تحميل الملفات لمساحات عمل الذكاء الاصطناعي: الصور والصوت والفيديو والمستندات. يدعم جميع أنواع الملفات الشائعة المتعلقة بالذكاء الاصطناعي مع إنشاء الصور المصغرة التلقائي وتحويل WebP.",
        ["es"] = "Cargar archivos para espacios de trabajo de IA: imágenes, audio, video y documentos. Admite todos los tipos de archivos comunes relacionados con IA con generación automática de miniaturas y conversión WebP."
    };
}
