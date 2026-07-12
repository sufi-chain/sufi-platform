namespace SufiChain.SufiPlatform.Calendar.Data;

/// <summary>
/// Culture-specific MCP tool labels for Calendar AI tools.
/// Keys follow <c>MCPTool:{toolName}:DisplayName</c> and <c>MCPTool:{toolName}:Description</c>.
/// </summary>
public static class CalendarMcpToolSeedTexts
{
    public const string ListCalendars = "calendar.list_calendars";
    public const string GetCurrentTime = "calendar.get_current_time";
    public const string GetWorkingHours = "calendar.get_working_hours";
    public const string GetFreeBusy = "calendar.get_free_busy";
    public const string FindFreeSlots = "calendar.find_free_slots";
    public const string SearchEvents = "calendar.search_events";
    public const string CreateEvent = "calendar.create_event";
    public const string MoveEvent = "calendar.move_event";
    public const string MoveOccurrence = "calendar.move_occurrence";
    public const string CancelEvent = "calendar.cancel_event";
    public const string CancelOccurrence = "calendar.cancel_occurrence";
    public const string TestAvailability = "calendar.test_availability";

    public static IReadOnlyList<string> ToolNames { get; } =
    [
        ListCalendars,
        GetCurrentTime,
        GetWorkingHours,
        GetFreeBusy,
        FindFreeSlots,
        SearchEvents,
        CreateEvent,
        MoveEvent,
        MoveOccurrence,
        CancelEvent,
        CancelOccurrence,
        TestAvailability
    ];

    private static readonly Dictionary<string, McpToolSeedTextSet> Texts =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [ListCalendars] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "فهرست تقویم‌ها",
                    en: "List Calendars",
                    ar: "قائمة التقاويم",
                    es: "Listar calendarios"),
                Descriptions: Cultures(
                    fa: "تقویم‌های قابل مشاهده را با شناسه، نام، نوع، منطقه زمانی و پرچم پیش‌فرض فهرست می‌کند. وقتی calendarId نامشخص است ابتدا از این ابزار استفاده کنید.",
                    en: "Lists visible calendars with id, name, kind, time zone, owner type, and default flag. Use first when calendarId is unknown.",
                    ar: "يعرض التقاويم المرئية مع المعرّف والاسم والنوع والمنطقة الزمنية والعلم الافتراضي. استخدمه أولاً عندما يكون calendarId غير معروف.",
                    es: "Lista calendarios visibles con id, nombre, tipo, zona horaria y marca predeterminada. Úselo primero cuando calendarId sea desconocido.")),

            [GetCurrentTime] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "زمان فعلی",
                    en: "Get Current Time",
                    ar: "الوقت الحالي",
                    es: "Obtener hora actual"),
                Descriptions: Cultures(
                    fa: "زمان معتبر سرور را با تاریخ/ساعت محلی، روز هفته و تاریخ جلالی/میلادی برمی‌گرداند. قبل از هر عبارت تاریخ نسبی یا فارسی این ابزار را فراخوانی کنید.",
                    en: "Returns authoritative server now with local date/time, weekday, and Jalali/Gregorian fields. Call before any relative or localized date phrase.",
                    ar: "يعيد الوقت الموثوق للخادم مع التاريخ/الوقت المحلي ويوم الأسبوع والحقول الهجرية/الميلادية. استدعِه قبل أي عبارة تاريخ نسبية.",
                    es: "Devuelve la hora autorizada del servidor con fecha/hora local, día de la semana y campos jalali/gregorianos. Llámelo antes de cualquier fecha relativa.")),

            [GetWorkingHours] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "ساعات کاری",
                    en: "Get Working Hours",
                    ar: "ساعات العمل",
                    es: "Obtener horario laboral"),
                Descriptions: Cultures(
                    fa: "قوانین ساعات کاری/اداری یک تقویم را برمی‌گرداند. به calendarId نیاز دارد؛ اگر نامشخص است ابتدا calendar.list_calendars را فراخوانی کنید.",
                    en: "Gets working-hour or business-hour rules for a calendar. Requires calendarId; if unknown, call calendar.list_calendars first.",
                    ar: "يعيد قواعد ساعات العمل لتقويم. يتطلب calendarId؛ إذا كان غير معروف فاستدعِ calendar.list_calendars أولاً.",
                    es: "Obtiene reglas de horario laboral de un calendario. Requiere calendarId; si es desconocido, llame primero a calendar.list_calendars.")),

            [GetFreeBusy] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "آزاد/مشغول",
                    en: "Get Free/Busy",
                    ar: "التوفر/الانشغال",
                    es: "Obtener libre/ocupado"),
                Descriptions: Cultures(
                    fa: "بلوک‌های مشغول و زمان‌های آزاد را در بازه UTC برمی‌گرداند. قبل از تبدیل تاریخ‌های نسبی یا جلالی calendar.get_current_time را فراخوانی کنید.",
                    en: "Gets busy blocks and free slots in a UTC range. Call calendar.get_current_time before converting relative or Jalali dates to UTC.",
                    ar: "يعيد فترات الانشغال والفراغ في نطاق UTC. استدعِ calendar.get_current_time قبل تحويل التواريخ النسبية أو الهجرية.",
                    es: "Obtiene bloques ocupados y huecos libres en un rango UTC. Llame a calendar.get_current_time antes de convertir fechas relativas o jalali.")),

            [FindFreeSlots] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "یافتن زمان آزاد",
                    en: "Find Free Slots",
                    ar: "إيجاد فترات فارغة",
                    es: "Buscar huecos libres"),
                Descriptions: Cultures(
                    fa: "زمان‌های آزاد را در بازه UTC پیدا می‌کند. قبل از تبدیل تاریخ‌های نسبی یا جلالی calendar.get_current_time را فراخوانی کنید.",
                    en: "Finds available slots in a UTC range. Call calendar.get_current_time before converting relative or Jalali dates to UTC.",
                    ar: "يجد الفترات المتاحة في نطاق UTC. استدعِ calendar.get_current_time قبل تحويل التواريخ النسبية أو الهجرية.",
                    es: "Encuentra huecos disponibles en un rango UTC. Llame a calendar.get_current_time antes de convertir fechas relativas o jalali.")),

            [SearchEvents] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "جستجوی رویدادها",
                    en: "Search Events",
                    ar: "بحث الأحداث",
                    es: "Buscar eventos"),
                Descriptions: Cultures(
                    fa: "رویدادها را برای ویرایش، جابجایی یا لغو جستجو می‌کند. قبل از تبدیل بازه تاریخ calendar.get_current_time را فراخوانی کنید.",
                    en: "Searches events for update, move, or cancel. Call calendar.get_current_time before building fromUtc/toUtc ranges.",
                    ar: "يبحث عن الأحداث للتحديث أو النقل أو الإلغاء. استدعِ calendar.get_current_time قبل بناء نطاق fromUtc/toUtc.",
                    es: "Busca eventos para actualizar, mover o cancelar. Llame a calendar.get_current_time antes de definir fromUtc/toUtc.")),

            [CreateEvent] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "ایجاد رویداد",
                    en: "Create Event",
                    ar: "إنشاء حدث",
                    es: "Crear evento"),
                Descriptions: Cultures(
                    fa: "رویداد تقویم ایجاد می‌کند. ابتدا calendar.list_calendars و calendar.get_current_time را فراخوانی کنید و زمان محلی را به UTC تبدیل کنید.",
                    en: "Creates a calendar event. Call calendar.list_calendars and calendar.get_current_time first, then convert local times to UTC.",
                    ar: "ينشئ حدث تقويم. استدعِ calendar.list_calendars و calendar.get_current_time أولاً ثم حوّل الأوقات المحلية إلى UTC.",
                    es: "Crea un evento de calendario. Llame primero a calendar.list_calendars y calendar.get_current_time y convierta horas locales a UTC.")),

            [MoveEvent] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "جابجایی رویداد",
                    en: "Move Event",
                    ar: "نقل حدث",
                    es: "Mover evento"),
                Descriptions: Cultures(
                    fa: "یک رویداد غیرتکراری را جابجا می‌کند. eventId باید از نتایج ابزار باشد؛ در صورت نیاز ابتدا calendar.search_events را فراخوانی کنید.",
                    en: "Moves a non-recurring event. eventId must come from tool results; call calendar.search_events first when needed.",
                    ar: "ينقل حدثاً غير متكرر. يجب أن يأتي eventId من نتائج الأداة؛ استدعِ calendar.search_events عند الحاجة.",
                    es: "Mueve un evento no recurrente. eventId debe provenir de resultados de herramientas; llame a calendar.search_events si hace falta.")),

            [MoveOccurrence] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "جابجایی تکرار",
                    en: "Move Occurrence",
                    ar: "نقل تكرار",
                    es: "Mover ocurrencia"),
                Descriptions: Cultures(
                    fa: "یک تکرار رویداد را جابجا می‌کند. eventId و originalStartUtc باید از نتایج ابزار باشند.",
                    en: "Moves a recurring occurrence. eventId and originalStartUtc must come from tool results.",
                    ar: "ينقل تكرار حدث. يجب أن يأتي eventId و originalStartUtc من نتائج الأداة.",
                    es: "Mueve una ocurrencia recurrente. eventId y originalStartUtc deben provenir de resultados de herramientas.")),

            [CancelEvent] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "لغو رویداد",
                    en: "Cancel Event",
                    ar: "إلغاء حدث",
                    es: "Cancelar evento"),
                Descriptions: Cultures(
                    fa: "یک رویداد را لغو می‌کند. eventId باید از نتایج ابزار باشد؛ در صورت نیاز ابتدا calendar.search_events را فراخوانی کنید.",
                    en: "Cancels an event. eventId must come from tool results; call calendar.search_events first when needed.",
                    ar: "يلغي حدثاً. يجب أن يأتي eventId من نتائج الأداة؛ استدعِ calendar.search_events عند الحاجة.",
                    es: "Cancela un evento. eventId debe provenir de resultados de herramientas; llame a calendar.search_events si hace falta.")),

            [CancelOccurrence] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "لغو تکرار",
                    en: "Cancel Occurrence",
                    ar: "إلغاء تكرار",
                    es: "Cancelar ocurrencia"),
                Descriptions: Cultures(
                    fa: "یک تکرار رویداد را لغو می‌کند. eventId و originalStartUtc باید از نتایج ابزار باشند.",
                    en: "Cancels a recurring occurrence. eventId and originalStartUtc must come from tool results.",
                    ar: "يلغي تكرار حدث. يجب أن يأتي eventId و originalStartUtc من نتائج الأداة.",
                    es: "Cancela una ocurrencia recurrente. eventId y originalStartUtc deben provenir de resultados de herramientas.")),

            [TestAvailability] = new McpToolSeedTextSet(
                DisplayNames: Cultures(
                    fa: "بررسی دسترس‌پذیری",
                    en: "Test Availability",
                    ar: "اختبار التوفر",
                    es: "Probar disponibilidad"),
                Descriptions: Cultures(
                    fa: "بررسی می‌کند تقویم در یک لحظه UTC باز است یا خیر و زمان باز/بسته بعدی را برمی‌گرداند.",
                    en: "Checks whether a calendar is open at a UTC instant and returns the next open/close times.",
                    ar: "يتحقق مما إذا كان التقويم مفتوحاً في لحظة UTC ويعيد أوقات الفتح/الإغلاق التالية.",
                    es: "Comprueba si un calendario está abierto en un instante UTC y devuelve las próximas horas de apertura/cierre."))
        };

    public static McpToolSeedTextSet Get(string toolName)
    {
        return Texts.TryGetValue(toolName, out var textSet)
            ? textSet
            : throw new KeyNotFoundException($"Missing MCP seed texts for tool '{toolName}'.");
    }

    private static IReadOnlyDictionary<string, string> Cultures(string fa, string en, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fa"] = fa,
            ["en"] = en,
            ["ar"] = ar,
            ["es"] = es
        };
}

public sealed record McpToolSeedTextSet(
    IReadOnlyDictionary<string, string> DisplayNames,
    IReadOnlyDictionary<string, string> Descriptions);
