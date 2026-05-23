using System.ComponentModel.DataAnnotations;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiAbp.TenantManagement;

public abstract class TenantCreateOrUpdateDtoBase : ExtensibleObject
{
    [Required]
    [MaxLength(64)]
    public virtual string Name { get; set; } = null!;
}
