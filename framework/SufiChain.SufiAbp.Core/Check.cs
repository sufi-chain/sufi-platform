namespace SufiChain.SufiAbp;

public static class Check
{
    public static T NotNull<T>(T value, string parameterName)
    {
        return Volo.Abp.Check.NotNull(value, parameterName);
    }

    public static T NotNull<T>(T value, string parameterName, string message)
    {
        return Volo.Abp.Check.NotNull(value, parameterName, message);
    }

    public static string NotNullOrWhiteSpace(string? value, string parameterName, int maxLength = int.MaxValue)
    {
        return Volo.Abp.Check.NotNullOrWhiteSpace(value, parameterName, maxLength);
    }
}
