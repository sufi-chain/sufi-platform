using JetBrains.Annotations;

namespace SufiChain.SufiAbp.Users;

public interface IUpdateUserData
{
    bool Update([NotNull] IUserData user);
}
