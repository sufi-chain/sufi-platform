using SufiChain.SufiAbp.ShortLinkGenerator.MongoDB;
using SufiChain.SufiAbp.ShortLinkGenerator.Samples;
using Xunit;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDb.Applications;

[Collection(MongoTestCollection.Name)]
public class MongoDBSampleAppService_Tests : SampleAppService_Tests<ShortLinkGeneratorMongoDbTestModule>
{

}
