using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Payment;

[Authorize]
public class PaymentMethodAppService : ApplicationService
{
    private readonly IRepository<PaymentMethod, Guid> _paymentMethodRepository;

    public PaymentMethodAppService(IRepository<PaymentMethod, Guid> paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<List<PaymentMethodDto>> GetListAsync()
    {
        var methods = await _paymentMethodRepository.GetListAsync();

        return methods.Select(x => new PaymentMethodDto
        {
            Id = x.Id,
            Name = x.Name,
            IsActive = x.IsActive
        }).ToList();
    }

    public async Task ToggleStatusAsync(Guid id, bool isActive)
    {
        var method = await _paymentMethodRepository.GetAsync(id);
        method.ToggleStatus(isActive);
        await _paymentMethodRepository.UpdateAsync(method);
    }
}
