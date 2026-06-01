namespace SufiChain.Chat;

public static class ChatTestData
{
    public static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid UserAId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid UserBId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid UserCId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public const string AnonymousVisitorId = "anon-visitor-001";
    public const string AnonymousVisitorId2 = "anon-visitor-002";
    public const string AnonymousIpHash = "ip-hash-001";
    public const string AnonymousIpHash2 = "ip-hash-002";
    public const string DefaultWorkspaceName = "test-workspace";
}
