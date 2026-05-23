using SufiChain.SufiAbp.ShortLinkGenerator.Samples;
using Xunit;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.Samples;

[Collection(MongoTestCollection.Name)]
public class SampleRepository_Tests : SampleRepository_Tests<ShortLinkGeneratorMongoDbTestModule>
{
    /* Don't write custom repository tests here, instead write to
     * the base class.
     * One exception can be some specific tests related to MongoDB.
     */
}
