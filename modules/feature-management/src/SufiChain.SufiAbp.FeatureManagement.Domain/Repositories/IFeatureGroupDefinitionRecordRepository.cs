using System;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.FeatureManagement;

public interface IFeatureGroupDefinitionRecordRepository : IBasicRepository<FeatureGroupDefinitionRecord, Guid>
{

}
