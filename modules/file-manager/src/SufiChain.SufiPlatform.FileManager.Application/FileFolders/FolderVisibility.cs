using System.Collections.Generic;
using System.Linq;
using SufiChain.SufiPlatform.FileManager.FileFolders;

namespace SufiChain.SufiPlatform.FileManager.FileFolders;

/// <summary>
/// Decides which folders a caller may see from one already-loaded tree.
/// A folder with no grants on itself or its ancestors stays visible.
/// A grant on the chain is required otherwise. Ancestors of a visible folder
/// stay in the tree as names so the caller can navigate to the grant.
/// </summary>
internal static class FolderVisibility
{
    public static Plan Build(
        IReadOnlyList<FileFolder> folders,
        FolderAccessContext context,
        IFolderAccessResolver resolver)
    {
        var byId = new Dictionary<Guid, FileFolder>(folders.Count);
        foreach (var folder in folders)
        {
            byId[folder.Id] = folder;
        }

        var readable = new HashSet<Guid>();
        foreach (var folder in folders)
        {
            if (CanRead(Chain(folder, byId), context, resolver))
            {
                readable.Add(folder.Id);
            }
        }

        var shown = new HashSet<Guid>(readable);
        foreach (var id in readable)
        {
            if (!byId.TryGetValue(id, out var current))
            {
                continue;
            }

            while (current.ParentId.HasValue && byId.TryGetValue(current.ParentId.Value, out var parent))
            {
                shown.Add(parent.Id);
                current = parent;
            }
        }

        return new Plan(readable, shown);
    }

    public static bool CanRead(
        IReadOnlyList<FileFolder> chain,
        FolderAccessContext context,
        IFolderAccessResolver resolver)
    {
        if (chain.Count == 0 || context.IsAnonymous)
        {
            return false;
        }

        var target = chain[chain.Count - 1];
        if (context.IsHost || context.IsAdmin || target.CreatorId == context.UserId)
        {
            return true;
        }

        var unrestricted = true;
        for (var i = 0; i < chain.Count; i++)
        {
            if (chain[i].Permissions != null && chain[i].Permissions.Count > 0)
            {
                unrestricted = false;
                break;
            }
        }

        if (unrestricted)
        {
            return true;
        }

        if (!context.IsHost && target.TenantId != context.TenantId)
        {
            return false;
        }

        return resolver.HasPermission(chain, context, FolderPermissionLevel.Read)
            || resolver.HasPermission(chain, context, FolderPermissionLevel.Write)
            || resolver.HasPermission(chain, context, FolderPermissionLevel.Delete)
            || resolver.HasPermission(chain, context, FolderPermissionLevel.Share);
    }

    public static bool CanWrite(
        IReadOnlyList<FileFolder> chain,
        FolderAccessContext context,
        IFolderAccessResolver resolver)
    {
        if (chain.Count == 0 || context.IsAnonymous)
        {
            return false;
        }

        var target = chain[chain.Count - 1];
        if (!context.IsHost && target.TenantId != context.TenantId)
        {
            return false;
        }

        if (context.IsHost || context.IsAdmin || target.CreatorId == context.UserId)
        {
            return true;
        }

        return resolver.HasPermission(chain, context, FolderPermissionLevel.Write);
    }

    public static bool CanShare(
        IReadOnlyList<FileFolder> chain,
        FolderAccessContext context,
        IFolderAccessResolver resolver)
    {
        if (!CanRead(chain, context, resolver))
        {
            return false;
        }

        if (context.IsHost || context.IsAdmin || chain[chain.Count - 1].CreatorId == context.UserId)
        {
            return true;
        }

        return resolver.HasPermission(chain, context, FolderPermissionLevel.Share);
    }

    public static List<FileFolder> Chain(FileFolder folder, IReadOnlyDictionary<Guid, FileFolder> byId)
    {
        var chain = new List<FileFolder>();
        var current = folder;
        while (current != null)
        {
            chain.Add(current);
            if (!current.ParentId.HasValue || !byId.TryGetValue(current.ParentId.Value, out var parent))
            {
                break;
            }

            current = parent;
        }

        chain.Reverse();
        return chain;
    }

    internal sealed class Plan
    {
        private readonly HashSet<Guid> _readable;
        private readonly HashSet<Guid> _shown;

        public Plan(HashSet<Guid> readable, HashSet<Guid> shown)
        {
            _readable = readable;
            _shown = shown;
        }

        public bool Shows(Guid id) => _shown.Contains(id);

        public bool CanRead(Guid id) => _readable.Contains(id);

        public bool NameOnly(Guid id) => _shown.Contains(id) && !_readable.Contains(id);
    }
}
