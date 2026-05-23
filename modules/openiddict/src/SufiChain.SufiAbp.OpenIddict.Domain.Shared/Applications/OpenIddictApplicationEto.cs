using System;

namespace SufiChain.SufiAbp.OpenIddict.Applications;

public class OpenIddictApplicationEto
{
    public Guid Id { get; set; }

    public string? ClientId { get; set; }
}

public class OpenIddictApplicationClientIdChangedEto
{
    public Guid Id { get; set; }

    public string? OldClientId { get; set; }

    public string? ClientId { get; set; }
}
