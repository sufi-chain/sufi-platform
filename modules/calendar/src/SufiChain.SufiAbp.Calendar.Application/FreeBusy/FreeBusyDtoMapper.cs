namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public static class FreeBusyDtoMapper
{
    public static FreeBusyResultDto ToDto(FreeBusyResult result)
    {
        return new FreeBusyResultDto
        {
            FromUtc = result.FromUtc,
            ToUtc = result.ToUtc,
            BusyBlocks = result.BusyBlocks.Select(ToDto).ToList(),
            FreeSlots = result.FreeSlots.Select(ToDto).ToList()
        };
    }

    public static FreeBusySlotDto ToDto(BusyBlock block)
    {
        return new FreeBusySlotDto
        {
            CalendarId = block.CalendarId,
            StartUtc = block.StartUtc,
            EndUtc = block.EndUtc,
            BusyCount = block.BusyCount,
            MaxConcurrent = block.MaxConcurrent,
            IsCapacityFull = block.IsCapacityFull
        };
    }

    public static FreeBusySlotDto ToDto(FreeSlot slot)
    {
        return new FreeBusySlotDto
        {
            CalendarId = slot.CalendarId,
            StartUtc = slot.StartUtc,
            EndUtc = slot.EndUtc,
            BusyCount = 0,
            MaxConcurrent = slot.MaxConcurrent,
            IsCapacityFull = false
        };
    }
}
