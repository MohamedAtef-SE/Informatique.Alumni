using Xunit;

namespace Informatique.Alumni.EntityFrameworkCore;

[CollectionDefinition(AlumniTestConsts.CollectionDefinitionName)]
public class AlumniEntityFrameworkCoreCollection : ICollectionFixture<AlumniEntityFrameworkCoreFixture>
{

}
