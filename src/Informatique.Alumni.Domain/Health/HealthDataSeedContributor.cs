using System;
using System.Threading.Tasks;
using Informatique.Alumni.Health;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Informatique.Alumni.Data;

public class HealthDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<MedicalPartner, Guid> _partnerRepository;
    private readonly IGuidGenerator _guidGenerator;

    public HealthDataSeedContributor(
        IRepository<MedicalPartner, Guid> partnerRepository,
        IGuidGenerator guidGenerator)
    {
        _partnerRepository = partnerRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _partnerRepository.GetCountAsync() > 0)
        {
            return;
        }

        // Seed Verified Partner 1
        var partner1 = new MedicalPartner(
            _guidGenerator.Create(),
            "Cairo Medical Center",
            MedicalPartnerType.Hospital,
            "Nasr City, Cairo",
            "+201001234567"
        );
        partner1.SetPremiumDetails(null, "Cairo", "Nasr City", "General Hospital", "contact@cmc.com", "19001", true);
        partner1.AddOffer(_guidGenerator.Create(), "Full Checkup", "Comprehensive medical checkup", null, 25);
        partner1.AddOffer(_guidGenerator.Create(), "Dental Cleaning", "Professional dental cleaning", null, 15);
        await _partnerRepository.InsertAsync(partner1);

        // Seed Verified Partner 2
        var partner2 = new MedicalPartner(
            _guidGenerator.Create(),
            "Elite Dental Clinic",
            MedicalPartnerType.Clinic,
            "Maadi, Cairo",
            "+201112223334"
        );
        partner2.SetPremiumDetails(null, "Cairo", "Maadi", "Dental Services", "info@elitedental.com", "19002", true);
        partner2.AddOffer(_guidGenerator.Create(), "Teeth Whitening", "Professional teeth whitening", null, 30);
        await _partnerRepository.InsertAsync(partner2);

        // Seed Unverified Partner
        var partner3 = new MedicalPartner(
            _guidGenerator.Create(),
            "Local Pharmacy Network",
            MedicalPartnerType.Pharmacy,
            "Various Locations",
            "+201223334445"
        );
        partner3.SetPremiumDetails(null, "Multiple", "All Regions", "Pharmacy Chain", "help@localpharmacy.com", "19003", false);
        partner3.AddOffer(_guidGenerator.Create(), "Medicine Discount", "Discount on all medicines", "DIAL10", 10);
        await _partnerRepository.InsertAsync(partner3);

        // Stats should be:
        // Verified Quality: 2/3 = 66.6%
        // Average Savings: (25 + 15 + 30 + 10) / 4 = 80 / 4 = 20%
    }
}
