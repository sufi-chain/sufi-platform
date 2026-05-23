namespace SufiChain.SufiAbp.UI.Navigation;

/// <summary>
/// A list of menu items with normalization support.
/// </summary>
public class ApplicationMenuItemList : List<ApplicationMenuItem>
{
    public ApplicationMenuItemList()
    {
    }

    public ApplicationMenuItemList(int capacity)
        : base(capacity)
    {
    }

    public ApplicationMenuItemList(IEnumerable<ApplicationMenuItem> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Normalizes the list by removing empty items and sorting by order.
    /// </summary>
    public void Normalize()
    {
        RemoveEmptyItems();
        OrderItems();
    }

    private void RemoveEmptyItems()
    {
        RemoveAll(item => item.IsLeaf && string.IsNullOrEmpty(item.Url));
    }

    private void OrderItems()
    {
        var orderedItems = this.OrderBy(item => item.Order).ToArray();
        Clear();
        AddRange(orderedItems);
    }
}
