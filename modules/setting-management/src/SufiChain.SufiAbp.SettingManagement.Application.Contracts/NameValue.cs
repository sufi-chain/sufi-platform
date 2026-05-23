namespace SufiChain.SufiAbp.SettingManagement;

public class NameValue
{
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public NameValue()
    {
    }

    public NameValue(string name, string value)
    {
        Name = name;
        Value = value;
    }
}
