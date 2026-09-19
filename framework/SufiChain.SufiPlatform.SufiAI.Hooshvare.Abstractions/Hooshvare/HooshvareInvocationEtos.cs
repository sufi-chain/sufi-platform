using SufiChain.SufiPlatform.EventBus;

using Volo.Abp.EventBus;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



/// <summary>

/// Request that a platform hooshvare produce a response (async invocation path).

/// </summary>

[Serializable]

[EventName("SufiAI.Hooshvare.InvocationRequested")]

public class HooshvareInvocationRequestedEto : SufiIntegrationEto

{

    public string HooshvareKey { get; set; } = string.Empty;



    public Guid? HooshvareId { get; set; }



    public string Message { get; set; } = string.Empty;



    public Guid? SessionId { get; set; }



    public string? LinkedEntityType { get; set; }



    public string? LinkedEntityId { get; set; }



    public string? MetadataJson { get; set; }

}



/// <summary>

/// Hooshvare response ready for the requesting feature module.

/// </summary>

[Serializable]

[EventName("SufiAI.Hooshvare.ResponseReady")]

public class HooshvareResponseReadyEto : SufiIntegrationEto

{

    public string HooshvareKey { get; set; } = string.Empty;



    public Guid HooshvareId { get; set; }



    public Guid? SessionId { get; set; }



    public string ResponseText { get; set; } = string.Empty;



    public bool Success { get; set; }



    public string? ErrorMessage { get; set; }



}

