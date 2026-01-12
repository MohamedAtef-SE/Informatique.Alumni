using Informatique.Alumni.Samples;
using Xunit;

namespace Informatique.Alumni.EntityFrameworkCore.Applications;

[Collection(AlumniTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AlumniEntityFrameworkCoreTestModule>
{

}
