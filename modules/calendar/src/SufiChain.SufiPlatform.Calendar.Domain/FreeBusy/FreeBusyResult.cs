namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public sealed record FreeBusyResult(DateTime FromUtc, DateTime ToUtc, IReadOnlyList<BusyBlock> BusyBlocks, IReadOnlyList<FreeSlot> FreeSlots);
