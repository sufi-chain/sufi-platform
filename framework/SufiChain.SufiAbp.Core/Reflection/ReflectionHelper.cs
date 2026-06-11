namespace SufiChain.SufiAbp.Reflection;

public static class ReflectionHelper
{
    public static string[] GetPublicConstantsRecursively(Type type)
    {
        return Volo.Abp.Reflection.ReflectionHelper.GetPublicConstantsRecursively(type);
    }
}
