namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public sealed record FreeBusyResult(DateTime FromUtc, DateTime ToUtc, IReadOnlyList<BusyBlock> BusyBlocks, IReadOnlyList<FreeSlot> FreeSlots);
