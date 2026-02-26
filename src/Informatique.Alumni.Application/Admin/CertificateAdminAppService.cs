using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Certificates;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.CertificateManage)]
public class CertificateAdminAppService : AlumniAppService, ICertificateAdminAppService
{
    private readonly IRepository<CertificateRequest, Guid> _requestRepository;

    public CertificateAdminAppService(IRepository<CertificateRequest, Guid> requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<PagedResultDto<CertificateRequestAdminDto>> GetRequestsAsync(CertificateAdminGetListInput input)
    {
        var queryable = await _requestRepository.GetQueryableAsync();

        if (input.StatusFilter.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.StatusFilter.Value);
        }

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input);

        var items = queryable.ToList().Select(r => new CertificateRequestAdminDto
        {
            Id = r.Id,
            AlumniId = r.AlumniId,
            Status = r.Status,
            DeliveryMethod = r.DeliveryMethod,
            TotalItemFees = r.TotalItemFees,
            DeliveryFee = r.DeliveryFee,
            AdminNotes = r.AdminNotes,
            CreationTime = r.CreationTime
        }).ToList();

        return new PagedResultDto<CertificateRequestAdminDto>(totalCount, items);
    }

    public async Task MoveToProcessingAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.MoveToProcessing();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task MarkAsReadyForPickupAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.MarkAsReadyForPickup();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task MarkAsOutForDeliveryAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.MarkAsOutForDelivery();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task DeliverAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Deliver();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task RejectAsync(Guid id, string reason)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Reject(reason);
        await _requestRepository.UpdateAsync(request);
    }
}
