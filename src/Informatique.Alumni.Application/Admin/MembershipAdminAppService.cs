using System;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.MembershipManage)]
public class MembershipAdminAppService : AlumniAppService, IMembershipAdminAppService
{
    private readonly IRepository<AssociationRequest, Guid> _requestRepository;

    public MembershipAdminAppService(IRepository<AssociationRequest, Guid> requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<PagedResultDto<MembershipRequestAdminDto>> GetRequestsAsync(MembershipAdminGetListInput input)
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

        var items = queryable.ToList().Select(r => new MembershipRequestAdminDto
        {
            Id = r.Id,
            AlumniId = r.AlumniId,
            Status = r.Status,
            RequestDate = r.RequestDate,
            ApprovalDate = r.ApprovalDate,
            RejectionReason = r.RejectionReason,
            DeliveryFee = r.DeliveryFee,
            DeliveryMethod = r.DeliveryMethod,
            ValidityStartDate = r.ValidityStartDate,
            ValidityEndDate = r.ValidityEndDate,
            CreationTime = r.CreationTime
        }).ToList();

        return new PagedResultDto<MembershipRequestAdminDto>(totalCount, items);
    }

    public async Task ApproveAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Approve();
        await _requestRepository.UpdateAsync(request);
    }

    public async Task RejectAsync(Guid id, string reason)
    {
        var request = await _requestRepository.GetAsync(id);
        request.Reject(reason);
        await _requestRepository.UpdateAsync(request);
    }

    public async Task MarkAsPaidAsync(Guid id)
    {
        var request = await _requestRepository.GetAsync(id);
        request.MarkAsPaid();
        await _requestRepository.UpdateAsync(request);
    }
}
