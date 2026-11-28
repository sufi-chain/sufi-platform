namespace SufiChain.SufiPlatform.Menus.Menus;

public static class MenuSiblingOrder
{
    public static IReadOnlyList<MenuItem> Place(IReadOnlyList<MenuItem> siblings, Guid movedId, int targetIndex)
    {
        var ordered = Sort(siblings);
        var moved = ordered.First(item => item.Id == movedId);
        ordered.Remove(moved);
        var index = Math.Clamp(targetIndex, 0, ordered.Count);
        ordered.Insert(index, moved);
        Write(ordered);
        return ordered;
    }

    public static IReadOnlyList<MenuItem> Normalize(IReadOnlyList<MenuItem> siblings)
    {
        var ordered = Sort(siblings);
        Write(ordered);
        return ordered;
    }

    private static List<MenuItem> Sort(IReadOnlyList<MenuItem> siblings) =>
        siblings
            .OrderBy(item => item.DisplayOrder)
            .ThenBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Id)
            .ToList();

    private static void Write(List<MenuItem> ordered)
    {
        for (var index = 0; index < ordered.Count; index++)
        {
            if (ordered[index].DisplayOrder != index)
            {
                ordered[index].Reorder(index);
            }
        }
    }
}
