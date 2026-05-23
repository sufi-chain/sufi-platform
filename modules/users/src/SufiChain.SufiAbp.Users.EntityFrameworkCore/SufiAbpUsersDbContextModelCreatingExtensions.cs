using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SufiChain.SufiAbp.Users;

public static class SufiAbpUsersDbContextModelCreatingExtensions
{
    public static void ConfigureAbpUser<TUser>(this EntityTypeBuilder<TUser> b)
        where TUser : class, IUser
    {
        b.Property(u => u.TenantId).HasColumnName(nameof(IUser.TenantId));
        b.Property(u => u.UserName).IsRequired().HasMaxLength(SufiAbpUserConsts.MaxUserNameLength).HasColumnName(nameof(IUser.UserName));
        b.Property(u => u.Email).IsRequired().HasMaxLength(SufiAbpUserConsts.MaxEmailLength).HasColumnName(nameof(IUser.Email));
        b.Property(u => u.Name).HasMaxLength(SufiAbpUserConsts.MaxNameLength).HasColumnName(nameof(IUser.Name));
        b.Property(u => u.Surname).HasMaxLength(SufiAbpUserConsts.MaxSurnameLength).HasColumnName(nameof(IUser.Surname));
        b.Property(u => u.EmailConfirmed).HasDefaultValue(false).HasColumnName(nameof(IUser.EmailConfirmed));
        b.Property(u => u.PhoneNumber).HasMaxLength(SufiAbpUserConsts.MaxPhoneNumberLength).HasColumnName(nameof(IUser.PhoneNumber));
        b.Property(u => u.PhoneNumberConfirmed).HasDefaultValue(false).HasColumnName(nameof(IUser.PhoneNumberConfirmed));
        b.Property(u => u.IsActive).HasColumnName(nameof(IUser.IsActive));
    }
}
