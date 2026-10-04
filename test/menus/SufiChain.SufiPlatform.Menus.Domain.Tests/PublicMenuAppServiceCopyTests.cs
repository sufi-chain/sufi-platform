using System.Collections;
using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.Menus.Menus;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class PublicMenuAppServiceCopyTests
{
    [Fact]
    public void CopyItem_Should_Keep_CultureUrls()
    {
        var source = new MenuItemDto
        {
            Id = Guid.NewGuid(),
            Name = "solve",
            DisplayName = "Solve",
            Url = "/#solve",
            CultureUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = "/#opensource",
                ["ar"] = "/#top"
            }
        };

        var copy = new MenuItemDto();
        PublicMenuAppService.CopyItem(source, copy);

        copy.Url.ShouldBe("/#solve");
        copy.CultureUrls.ShouldNotBeNull();
        copy.CultureUrls.ShouldNotBeSameAs(source.CultureUrls);
        copy.CultureUrls["en"].ShouldBe("/#opensource");
        copy.CultureUrls["ar"].ShouldBe("/#top");

        var empty = new MenuItemDto { Name = "home", DisplayName = "Home", Url = "/" };
        var emptyCopy = new MenuItemDto();
        PublicMenuAppService.CopyItem(empty, emptyCopy);
        emptyCopy.CultureUrls.ShouldBeNull();
    }

    [Fact]
    public void CopyItem_Should_Copy_Every_Field()
    {
        var properties = typeof(MenuItemDto)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0)
            .ToList();
        properties.ShouldNotBeEmpty();

        var source = new MenuItemDto();
        foreach (var property in properties)
        {
            property.SetValue(source, Sample(property));
        }

        var target = new MenuItemDto();
        PublicMenuAppService.CopyItem(source, target);

        foreach (var property in properties)
        {
            var expected = property.GetValue(source);
            var actual = property.GetValue(target);
            if (expected is IDictionary expectedMap)
            {
                var actualMap = actual.ShouldBeAssignableTo<IDictionary>();
                actualMap.ShouldNotBeSameAs(expectedMap);
                actualMap.Count.ShouldBe(expectedMap.Count);
                foreach (DictionaryEntry entry in expectedMap)
                {
                    actualMap[entry.Key].ShouldBe(entry.Value);
                }
            }
            else
            {
                actual.ShouldBe(expected);
            }
        }
    }

    private static object Sample(PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (type == typeof(string))
        {
            return property.Name + "-value";
        }

        if (type == typeof(Guid))
        {
            return Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        if (type == typeof(int))
        {
            return 42;
        }

        if (type == typeof(bool))
        {
            return true;
        }

        if (type == typeof(DateTime))
        {
            return new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        }

        if (type.IsEnum)
        {
            return Enum.GetValues(type).Cast<object>().Last();
        }

        if (type == typeof(Dictionary<string, string>))
        {
            return new Dictionary<string, string> { [property.Name] = property.Name + "-url" };
        }

        throw new InvalidOperationException($"No sample for {property.Name} ({property.PropertyType}).");
    }
}
