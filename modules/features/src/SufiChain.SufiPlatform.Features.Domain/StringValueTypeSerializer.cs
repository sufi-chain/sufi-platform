using Volo.Abp.DependencyInjection;
using Volo.Abp.Json;
using AbpIStringValueType = Volo.Abp.Validation.StringValues.IStringValueType;

namespace SufiChain.SufiPlatform.Features;

public class StringValueTypeSerializer : ITransientDependency
{
    protected IJsonSerializer JsonSerializer { get; }

    public StringValueTypeSerializer(IJsonSerializer jsonSerializer)
    {
        JsonSerializer = jsonSerializer;
    }

    public virtual string Serialize(AbpIStringValueType stringValueType)
    {
        return JsonSerializer.Serialize(stringValueType);
    }

    public virtual AbpIStringValueType Deserialize(string value)
    {
        return JsonSerializer.Deserialize<AbpIStringValueType>(value);
    }
}
