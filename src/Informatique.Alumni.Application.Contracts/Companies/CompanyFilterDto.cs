using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Companies;

public class CompanyFilterDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
