using AutoMapper;
using Informatique.Alumni.Health;

namespace Informatique.Alumni;

public class HealthcareApplicationAutoMapperProfile : Profile
{
    public HealthcareApplicationAutoMapperProfile()
    {
        CreateMap<HealthcareOffer, HealthcareOfferDto>();
    }
}
