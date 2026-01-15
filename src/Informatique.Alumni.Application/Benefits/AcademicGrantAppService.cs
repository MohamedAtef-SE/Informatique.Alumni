
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Benefits;

[Authorize]
public class AcademicGrantAppService : AlumniAppService
{
    private readonly IRepository<AcademicDiscount, Guid> _discountRepository;

    public AcademicGrantAppService(IRepository<AcademicDiscount, Guid> discountRepository)
    {
        _discountRepository = discountRepository;
    }

    public async Task<List<AcademicDiscountDto>> GetListAsync(AcademicDiscountFilterDto input)
    {
        var query = await _discountRepository.GetQueryableAsync();

        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
        }

        // Default Sort: Alphabetical by Name (NameEn as default or both?) Rules say: "Alphabetical by Name".
        query = query.OrderBy(x => x.NameEn);

        var items = await AsyncExecuter.ToListAsync(query);

        return ObjectMapper.Map<List<AcademicDiscount>, List<AcademicDiscountDto>>(items);
    }
}
