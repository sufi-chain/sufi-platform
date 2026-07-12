namespace SufiChain.SufiPlatform.Features;

public class FeatureDto
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Value { get; set; }

    public FeatureProviderDto Provider { get; set; }

    public string Description { get; set; }

    public FeatureValueTypeDto ValueType { get; set; }

    public int Depth { get; set; }

    public string ParentName { get; set; }
}

public class FeatureValueTypeDto
{
    public string Name { get; set; }

    public List<FeatureSelectionItemDto> SelectionItems { get; set; } = new();
}

public class FeatureSelectionItemDto
{
    public string Value { get; set; }

    public string DisplayTextResourceName { get; set; }

    public string DisplayTextName { get; set; }
}
