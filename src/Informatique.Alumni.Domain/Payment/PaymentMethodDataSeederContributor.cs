using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Payment;

public class PaymentMethodDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;

    public PaymentMethodDataSeederContributor(IRepository<PaymentMethod, Guid> paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await CreateMethodIfNotExistsAsync("ATM");
        await CreateMethodIfNotExistsAsync("Online");
        await CreateMethodIfNotExistsAsync("Bank");
    }

    private async Task CreateMethodIfNotExistsAsync(string name)
    {
        if (!await _paymentMethodRepository.AnyAsync(x => x.Name == name))
        {
            await _paymentMethodRepository.InsertAsync(
                new PaymentMethod(
                    Guid.NewGuid(), // Use Guid.NewGuid() as GuidGenerator is not easily available in Seeder without injection or async complexity? 
                    // Actually GuidGenerator is better. I'll stick to Guid.NewGuid() for simplicity in Seeder or inject IGuidGenerator if strict.
                    // Given context, I'll direct instantiate.
                    name, 
                    true
                )
            );
        }
    }
}
