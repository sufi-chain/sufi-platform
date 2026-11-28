using SufiChain.SufiPlatform.Identity;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Identity.Domain.Tests;

public class PermissionTreeGrantTests
{
    [Fact]
    public void TickParent_GrantsEnabledChildren()
    {
        var nodes = Sample();
        PermissionTreeGrant.Apply(true, nodes[0], nodes);
        nodes.Single(node => node.Name == "Tickets").IsGranted.ShouldBeTrue();
        nodes.Single(node => node.Name == "Tickets.My").IsGranted.ShouldBeTrue();
        nodes.Single(node => node.Name == "Tickets.Edit").IsGranted.ShouldBeTrue();
    }

    [Fact]
    public void TickParent_SkipsDisabledChildren()
    {
        var nodes = Sample();
        nodes.Single(node => node.Name == "Tickets.My").Disabled = true;
        PermissionTreeGrant.Apply(true, nodes[0], nodes);
        nodes.Single(node => node.Name == "Tickets.My").IsGranted.ShouldBeFalse();
        nodes.Single(node => node.Name == "Tickets.Edit").IsGranted.ShouldBeTrue();
    }

    [Fact]
    public void UntickParent_RevokesChildren()
    {
        var nodes = Sample();
        foreach (var node in nodes)
        {
            node.IsGranted = true;
        }

        PermissionTreeGrant.Apply(false, nodes[0], nodes);
        nodes.ShouldAllBe(node => !node.IsGranted);
    }

    [Fact]
    public void ParentWithSomeChildren_ShowsIndeterminate()
    {
        var nodes = Sample();
        nodes.Single(node => node.Name == "Tickets.Edit").IsGranted = true;
        PermissionTreeGrant.IsMixed(nodes[0], nodes).ShouldBeTrue();
        nodes.Single(node => node.Name == "Tickets.Edit").IsGranted = false;
        PermissionTreeGrant.IsMixed(nodes[0], nodes).ShouldBeFalse();
    }

    private static List<PermissionTreeGrant.Node> Sample() =>
    [
        new() { Name = "Tickets" },
        new() { Name = "Tickets.My", ParentName = "Tickets" },
        new() { Name = "Tickets.Edit", ParentName = "Tickets" }
    ];
}
