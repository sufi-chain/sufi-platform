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

    public static string NotNullOrEmpty(string? value, string parameterName, int maxLength = int.MaxValue)
    {
        return Volo.Abp.Check.NotNullOrEmpty(value, parameterName, maxLength);
    }

    public static ICollection<T> NotNullOrEmpty<T>(ICollection<T>? value, string parameterName)
    {
        return Volo.Abp.Check.NotNullOrEmpty(value, parameterName);
    }

    public static string? Length(string? value, string parameterName, int maxLength, int minLength = 0)
    {
        return Volo.Abp.Check.Length(value, parameterName, maxLength, minLength);
    }
}
