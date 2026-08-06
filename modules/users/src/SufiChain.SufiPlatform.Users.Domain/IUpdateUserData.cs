using JetBrains.Annotations;

namespace SufiChain.SufiPlatform.Users;

public interface IUpdateUserData
{
    bool Update([NotNull] IUserData user);
}
