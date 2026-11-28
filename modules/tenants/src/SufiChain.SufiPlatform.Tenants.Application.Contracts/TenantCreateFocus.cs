using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Tenants;

public static class TenantCreateFocus
{
    public static readonly string[] FieldOrder = ["Name", "AdminEmailAddress", "AdminPassword"];

    public static string? FirstInvalid(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        var members = results.SelectMany(result => result.MemberNames).ToHashSet(StringComparer.Ordinal);
        return FieldOrder.FirstOrDefault(members.Contains);
    }
}
