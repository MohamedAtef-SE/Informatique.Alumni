using System;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Users;

public class UserFilterDto : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    public Guid? BranchId { get; set; }
}
