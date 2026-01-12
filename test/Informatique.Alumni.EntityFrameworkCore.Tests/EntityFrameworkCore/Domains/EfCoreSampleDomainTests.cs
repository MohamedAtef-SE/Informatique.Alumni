using Informatique.Alumni.Samples;
using Xunit;

namespace Informatique.Alumni.EntityFrameworkCore.Domains;

[Collection(AlumniTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AlumniEntityFrameworkCoreTestModule>
{

}
