namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Parent and child grant rules for the role permission tree.
/// Ticking a parent grants its enabled children. Unticking revokes them.
/// A parent is mixed when some, but not all, enabled descendants are granted.
/// </summary>
public static class PermissionTreeGrant
{
    public sealed class Node
    {
        public string Name { get; set; } = string.Empty;
        public string? ParentName { get; set; }
        public bool IsGranted { get; set; }
        public bool Disabled { get; set; }
    }

    public static void Apply(bool value, Node permission, IReadOnlyList<Node> all)
    {
        if (permission.Disabled)
        {
            return;
        }

        permission.IsGranted = value;
        if (value)
        {
            GrantChildren(permission, all);
            GrantParents(permission, all);
        }
        else
        {
            RevokeChildren(permission, all);
        }
    }

    public static bool IsMixed(Node permission, IReadOnlyList<Node> all)
    {
        var descendants = Descendants(permission, all).Where(node => !node.Disabled).ToList();
        if (descendants.Count == 0)
        {
            return false;
        }

        var granted = descendants.Count(node => node.IsGranted);
        return granted > 0 && granted < descendants.Count;
    }

    private static void GrantChildren(Node permission, IReadOnlyList<Node> all)
    {
        foreach (var child in all.Where(node => node.ParentName == permission.Name))
        {
            if (child.Disabled)
            {
                continue;
            }

            if (!child.IsGranted)
            {
                child.IsGranted = true;
            }

            GrantChildren(child, all);
        }
    }

    private static void GrantParents(Node permission, IReadOnlyList<Node> all)
    {
        if (string.IsNullOrEmpty(permission.ParentName))
        {
            return;
        }

        var parent = all.FirstOrDefault(node => node.Name == permission.ParentName);
        if (parent != null && !parent.IsGranted)
        {
            parent.IsGranted = true;
            GrantParents(parent, all);
        }
    }

    private static void RevokeChildren(Node permission, IReadOnlyList<Node> all)
    {
        foreach (var child in all.Where(node => node.ParentName == permission.Name))
        {
            if (child.IsGranted && !child.Disabled)
            {
                child.IsGranted = false;
            }

            RevokeChildren(child, all);
        }
    }

    private static IEnumerable<Node> Descendants(Node permission, IReadOnlyList<Node> all)
    {
        foreach (var child in all.Where(node => node.ParentName == permission.Name))
        {
            yield return child;
            foreach (var nested in Descendants(child, all))
            {
                yield return nested;
            }
        }
    }
}
