using System;
using Volo.Abp;

namespace SufiChain.SufiAbp.Core;

public class NamedAction<T> : NamedObject
{
    public Action<T> Action { get; set; }
    
    public NamedAction(string name, Action<T> action)
    : base(name)
    {
        Action = Check.NotNull(action, nameof(action));
    }
}
