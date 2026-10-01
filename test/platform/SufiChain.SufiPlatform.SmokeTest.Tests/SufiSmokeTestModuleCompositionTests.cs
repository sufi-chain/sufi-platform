using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.SmokeTest.Console;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.SmokeTest;

public class SufiSmokeTestModuleCompositionTests
{
    [Fact]
    public void Module_Should_Compose_Account_Ai_And_Data_Owners_Without_Starting_The_Host()
    {
        var names = typeof(SufiSmokeTestModule)
            .GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType == typeof(DependsOnAttribute))
            .SelectMany(attribute => attribute.ConstructorArguments)
            .SelectMany(argument => (IReadOnlyList<CustomAttributeTypedArgument>)argument.Value!)
            .Select(argument => ((Type)argument.Value!).Name)
            .ToList();

        names.ShouldContain("SufiAccountHttpApiModule");
        names.ShouldContain("SufiAIApplicationModule");
        names.ShouldContain("SufiAIMongoDbModule");
        names.ShouldContain("SufiAuditLoggingMongoDbModule");
    }
}
