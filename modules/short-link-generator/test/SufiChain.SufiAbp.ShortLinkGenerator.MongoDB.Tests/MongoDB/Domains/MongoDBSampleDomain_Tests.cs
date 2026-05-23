using SufiChain.SufiAbp.ShortLinkGenerator.Samples;
using Xunit;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.Domains;

[Collection(MongoTestCollection.Name)]
public class MongoDBSampleDomain_Tests : SampleManager_Tests<ShortLinkGeneratorMongoDbTestModule>
{

}
