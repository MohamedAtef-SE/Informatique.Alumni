using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Membership;

public interface IMembershipAppService : IApplicationService
{
    Task<SubscriptionFeeDto> CreateSubscriptionFeeAsync(CreateSubscriptionFeeDto input);
    Task<ListResultDto<SubscriptionFeeDto>> GetActiveSubscriptionFeesAsync();
    
    Task<AssociationRequestDto> RequestMembershipAsync(CreateAssociationRequestDto input);
    Task<AssociationRequestDto> PayMembershipAsync(MembershipPaymentDto input);
    Task<AssociationRequestDto> ApproveRequestAsync(Guid id);
    Task RejectRequestAsync(Guid id, string reason);
    
    Task<PagedResultDto<AssociationRequestDto>> GetListAsync(MembershipRequestFilterDto input);
    Task UpdateStatusAsync(Guid id, UpdateStatusDto input);
    Task<CardPrintDto> GetCardDataAsync(Guid id);
}
