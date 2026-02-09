using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Health;

public class GetMedicalPartnersInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public bool? HasActiveOffers { get; set; }
    public MedicalPartnerType? Type { get; set; }
}
