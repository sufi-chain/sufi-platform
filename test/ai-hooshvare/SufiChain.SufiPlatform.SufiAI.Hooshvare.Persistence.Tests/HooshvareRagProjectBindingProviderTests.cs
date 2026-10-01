using SufiChain.SufiPlatform.SufiAI.Hooshvare;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class EfHooshvareRagProjectBindingIsolationTests
    : HooshvareRagProjectBindingIsolationTests<HooshvareRagBindingSqliteTestModule>
{
}

public class MongoHooshvareRagProjectBindingIsolationTests
    : HooshvareRagProjectBindingIsolationTests<HooshvareRagBindingMongoTestModule>
{
}
