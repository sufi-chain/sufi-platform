using System;

namespace SufiChain.SufiPlatform.Core.Collections;

public class NamedActionList<T> : NamedObjectList<NamedAction<T>>
{
    public void Add(Action<T> action)
    {
        Add(Guid.NewGuid().ToString("N"), action);
    }
    
    public void Add(string name, Action<T> action)
        => Add(new NamedAction<T>(name, action));
}
