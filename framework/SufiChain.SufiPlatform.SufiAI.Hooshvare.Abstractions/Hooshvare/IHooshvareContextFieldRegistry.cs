namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public interface IHooshvareContextFieldRegistry

{

    bool IsRegistered(string? hooshvareKey);



    IReadOnlyList<HooshvareContextFieldDescriptor> GetFields(string? hooshvareKey);

}

