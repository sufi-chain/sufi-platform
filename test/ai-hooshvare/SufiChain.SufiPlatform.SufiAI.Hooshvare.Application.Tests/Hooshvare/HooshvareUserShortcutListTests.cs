using System;

using System.Collections.Generic;

using System.Linq;

using Shouldly;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareUserShortcutListTests

{

    [Fact]

    public void SettingName_Should_Prefix_Hooshvare_Key()

    {

        HooshvareUserShortcutList.SettingName("SufiCalendar:Assistant")

            .ShouldBe("SufiAI.Hooshvare.UserShortcuts:SufiCalendar:Assistant");

    }



    [Fact]

    public void TryParse_Should_Treat_Missing_Json_As_Not_Customized()

    {

        HooshvareUserShortcutList.TryParse(null, out var items).ShouldBeFalse();

        items.ShouldBeEmpty();

    }



    [Fact]

    public void TryParse_Should_Treat_Empty_Array_As_Customized()

    {

        HooshvareUserShortcutList.TryParse("[]", out var items).ShouldBeTrue();

        items.ShouldBeEmpty();

    }



    [Fact]

    public void NormalizeStored_Should_Trim_Dedupe_And_Cap()

    {

        var tooLong = new string('a', HooshvareConsts.MaxUserShortcutPromptLength + 8);

        var items = new List<string>();

        for (var i = 0; i < HooshvareConsts.MaxUserShortcutsPerHooshvare + 3; i++)

        {

            items.Add($" Prompt {i} ");

        }



        items.Insert(0, tooLong);

        items.Add("prompt 0");



        var normalized = HooshvareUserShortcutList.NormalizeStored(items);

        normalized.Count.ShouldBe(HooshvareConsts.MaxUserShortcutsPerHooshvare);

        normalized[0].Length.ShouldBe(HooshvareConsts.MaxUserShortcutPromptLength);

        normalized.ShouldNotContain(item => item.StartsWith(" ", StringComparison.Ordinal));

        normalized.Count(item => item.Equals("Prompt 0", StringComparison.OrdinalIgnoreCase)).ShouldBe(1);

    }



    [Fact]

    public void Serialize_Should_Round_Trip()

    {

        var json = HooshvareUserShortcutList.Serialize(["What does my day look like?", "Am I free on Friday afternoon?"]);

        HooshvareUserShortcutList.TryParse(json, out var items).ShouldBeTrue();

        items.ShouldBe(["What does my day look like?", "Am I free on Friday afternoon?"]);

    }

}

