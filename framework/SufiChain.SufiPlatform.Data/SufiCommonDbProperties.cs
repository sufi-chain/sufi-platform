using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Shared Sufi Platform DB naming defaults for ABP common tables that still read
/// <see cref="AbpCommonDbProperties"/> (distributed event inbox/outbox).
/// Follows module practice: <c>SufiEvents.Inbox</c> / <c>SufiEvents.Outbox</c>.
/// </summary>
public static class SufiCommonDbProperties
{
    private static string _dbTablePrefix = "SufiEvents.";
    private static string? _dbSchema;

    static SufiCommonDbProperties()
    {
        ApplyToAbp();
    }

    /// <summary>
    /// Table prefix for distributed event inbox/outbox.
    /// Default: <c>SufiEvents.</c> (same dotted style as <c>SufiIdentity.</c>, <c>SufiCalendar.</c>, …).
    /// </summary>
    public static string DbTablePrefix
    {
        get => _dbTablePrefix;
        set
        {
            _dbTablePrefix = value;
            ApplyToAbp();
        }
    }

    /// <summary>
    /// Optional schema for those common tables. Default: <c>null</c>.
    /// </summary>
    public static string? DbSchema
    {
        get => _dbSchema;
        set
        {
            _dbSchema = value;
            ApplyToAbp();
        }
    }

    /// <summary>Final inbox table/collection name. Default: <c>SufiEvents.Inbox</c>.</summary>
    public static string EventInboxTableName => DbTablePrefix + "Inbox";

    /// <summary>Final outbox table/collection name. Default: <c>SufiEvents.Outbox</c>.</summary>
    public static string EventOutboxTableName => DbTablePrefix + "Outbox";

    /// <summary>
    /// Copies current Sufi defaults onto <see cref="AbpCommonDbProperties"/>.
    /// Safe to call multiple times (design-time EF, module startup, etc.).
    /// </summary>
    /// <remarks>
    /// ABP's <c>ConfigureEventInbox/Outbox</c> appends <c>EventInbox</c>/<c>EventOutbox</c>
    /// to <see cref="AbpCommonDbProperties.DbTablePrefix"/>. After calling those helpers,
    /// remap with <see cref="EventInboxTableName"/> / <see cref="EventOutboxTableName"/>
    /// so tables are <c>SufiEvents.Inbox</c> / <c>SufiEvents.Outbox</c>.
    /// </remarks>
    public static void ApplyToAbp()
    {
        AbpCommonDbProperties.DbTablePrefix = _dbTablePrefix;
        AbpCommonDbProperties.DbSchema = _dbSchema;
    }
}
