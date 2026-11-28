using Shouldly;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class MenuSiblingOrderTests
{
    private static readonly Guid MenuId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Fact]
    public void MoveUp_Middle_SwapsAndKeepsContiguous()
    {
        var items = Trio();
        var placed = MenuSiblingOrder.Place(items, items[1].Id, 0);
        Names(placed).ShouldBe(new[] { "B", "A", "C" });
        Orders(placed).ShouldBe(new[] { 0, 1, 2 });
    }

    [Fact]
    public void MoveDown_Middle_SwapsAndKeepsContiguous()
    {
        var items = Trio();
        var placed = MenuSiblingOrder.Place(items, items[1].Id, 2);
        Names(placed).ShouldBe(new[] { "A", "C", "B" });
        Orders(placed).ShouldBe(new[] { 0, 1, 2 });
    }

    [Fact]
    public void MoveUp_First_StaysFirst_NoNegative()
    {
        var items = Trio();
        var placed = MenuSiblingOrder.Place(items, items[0].Id, -1);
        Names(placed).ShouldBe(new[] { "A", "B", "C" });
        Orders(placed).ShouldBe(new[] { 0, 1, 2 });
    }

    [Fact]
    public void MoveDown_Last_StaysLast()
    {
        var items = Trio();
        var placed = MenuSiblingOrder.Place(items, items[2].Id, 99);
        Names(placed).ShouldBe(new[] { "A", "B", "C" });
        Orders(placed).ShouldBe(new[] { 0, 1, 2 });
    }

    [Fact]
    public void MoveToIndexBeyondEnd_ClampsToLast()
    {
        var items = Trio();
        var placed = MenuSiblingOrder.Place(items, items[0].Id, 50);
        Names(placed).ShouldBe(new[] { "B", "C", "A" });
        Orders(placed).ShouldBe(new[] { 0, 1, 2 });
    }

    [Fact]
    public void S19Repro_ThreeSiblings_EndAs0_1_2()
    {
        var a = Item("A", -1);
        var c = Item("C", -1);
        var b = Item("B", 0);
        Should.Throw<BusinessException>(() => a.Reorder(-1));
        a = Item("A", 0);
        c = Item("C", 0);
        SetOrder(a, -1);
        SetOrder(c, -1);
        var placed = MenuSiblingOrder.Normalize(new[] { a, c, b });
        Names(placed).ShouldBe(new[] { "A", "C", "B" });
        Orders(placed).ShouldBe(new[] { 0, 1, 2 });
    }

    [Fact]
    public void ChangeParent_RenormalizesOldAndNewParent()
    {
        var parent = Item("Parent", 0);
        var child = Item("Child", 4);
        child.SetParent(parent.Id);
        var sibling = Item("Sibling", 0);
        sibling.SetParent(parent.Id);
        var old = MenuSiblingOrder.Normalize(new[] { Item("Stay", 3) });
        var next = MenuSiblingOrder.Place(new[] { sibling, child }, child.Id, 0);
        Orders(old).ShouldBe(new[] { 0 });
        Names(next).ShouldBe(new[] { "Child", "Sibling" });
        Orders(next).ShouldBe(new[] { 0, 1 });
    }

    [Fact]
    public void Create_AppendsAtEnd()
    {
        var items = Trio();
        var created = Item("D", 0);
        var placed = MenuSiblingOrder.Place(items.Append(created).ToList(), created.Id, int.MaxValue);
        Names(placed).ShouldBe(new[] { "A", "B", "C", "D" });
        Orders(placed).ShouldBe(new[] { 0, 1, 2, 3 });
    }

    [Fact]
    public void ReadOrder_IsStableWithTies()
    {
        var first = Item("Same", 0);
        var second = Item("Same", 0);
        var placed = MenuSiblingOrder.Normalize(second.Id.CompareTo(first.Id) < 0
            ? new[] { first, second }
            : new[] { second, first });
        placed[0].Id.CompareTo(placed[1].Id).ShouldBeLessThan(0);
        Orders(placed).ShouldBe(new[] { 0, 1 });
    }

    private static MenuItem[] Trio() => [Item("A", 0), Item("B", 1), Item("C", 2)];

    private static MenuItem Item(string name, int order)
    {
        var item = new MenuItem(Guid.NewGuid(), MenuId, name, name, name.ToLowerInvariant());
        if (order >= 0)
        {
            item.Reorder(order);
        }

        return item;
    }

    private static void SetOrder(MenuItem item, int order)
    {
        typeof(MenuItem).GetProperty(nameof(MenuItem.DisplayOrder))!
            .SetValue(item, order);
    }

    private static string[] Names(IReadOnlyList<MenuItem> items) => items.Select(item => item.DisplayName).ToArray();

    private static int[] Orders(IReadOnlyList<MenuItem> items) => items.Select(item => item.DisplayOrder).ToArray();
}
