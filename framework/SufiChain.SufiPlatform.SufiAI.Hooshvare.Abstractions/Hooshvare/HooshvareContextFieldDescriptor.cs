namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public sealed class HooshvareContextFieldDescriptor

{

    public string Key { get; set; } = string.Empty;



    public HooshvareContextFieldKind ValueKind { get; set; }



    public bool Required { get; set; } = true;



    public string? DependsOn { get; set; }



    public string? Source { get; set; }



    public int? MaxLength { get; set; }



    public IReadOnlyList<string>? AllowedValues { get; set; }

}

