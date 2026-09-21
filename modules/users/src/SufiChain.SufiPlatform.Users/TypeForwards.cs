using System.Runtime.CompilerServices;
using SufiChain.SufiPlatform.Users;

[assembly: TypeForwardedTo(typeof(IUserData))]
[assembly: TypeForwardedTo(typeof(IRoleData))]
[assembly: TypeForwardedTo(typeof(IExternalUserLookupServiceProvider))]
[assembly: TypeForwardedTo(typeof(UserData))]
[assembly: TypeForwardedTo(typeof(RoleData))]
[assembly: TypeForwardedTo(typeof(UserEto))]
[assembly: TypeForwardedTo(typeof(UserPasswordChangeRequestedEto))]
[assembly: TypeForwardedTo(typeof(InviteUserToTenantRequestedEto))]
[assembly: TypeForwardedTo(typeof(SufiUsersAbstractionModule))]
[assembly: TypeForwardedTo(typeof(ExtraPropertyDictionaryExtensions))]
