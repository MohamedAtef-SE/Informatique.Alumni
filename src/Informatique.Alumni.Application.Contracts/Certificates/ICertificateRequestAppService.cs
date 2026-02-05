using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Informatique.Alumni.Certificates;

public interface ICertificateRequestAppService : IApplicationService
{
    Task<CertificateRequestDto> GetAsync(Guid id);

    Task<PagedResultDto<CertificateRequestDto>> GetListAsync(CertificateRequestFilterDto input);
    Task<PagedResultDto<CertificateRequestDto>> GetMyRequestsAsync(PagedAndSortedResultRequestDto input);
    Task<CertificateRequestDto> CreateAsync(CreateCertificateRequestDto input);
    Task<CertificateRequestDto> ProcessAsync(Guid id, ProcessCertificateRequestDto input);
    Task<CertificateRequestDto> RecordGatewayPaymentAsync(Guid id, RecordPaymentDto input);
    Task<CertificateRequestDto> UpdateStatusAsync(Guid id, UpdateCertificateStatusDto input);
    Task<CertificateRequestDto> GetByHashAsync(string hash);
}
