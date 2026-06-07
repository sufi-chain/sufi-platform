using Microsoft.Extensions.Localization;

namespace SufiChain.SufiAbp.Localization;

public class LocalizableString
{
    protected Volo.Abp.Localization.LocalizableString Inner { get; }

    public string Name => Inner.Name;

    public Type ResourceType => Inner.ResourceType;

    protected LocalizableString(Volo.Abp.Localization.LocalizableString inner)
    {
        Inner = inner;
    }

    public LocalizableString(Type resourceType, string name)
        : this(new Volo.Abp.Localization.LocalizableString(resourceType, name))
    {
    }

    public static LocalizableString Create<TResource>(string name)
    {
        return new LocalizableString(Volo.Abp.Localization.LocalizableString.Create<TResource>(name));
    }

    public static implicit operator Volo.Abp.Localization.LocalizableString(LocalizableString localizableString)
    {
        return localizableString.Inner;
    }

    public virtual Volo.Abp.Localization.LocalizableString ToVolo()
    {
        return Inner;
    }

    public virtual LocalizedString Localize(IStringLocalizerFactory stringLocalizerFactory)
    {
        return Inner.Localize(stringLocalizerFactory);
    }
}
