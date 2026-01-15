using System;
using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Certificates;

public interface ICertificateDefinitionAppService : 
    ICrudAppService<
        CertificateDefinitionDto, 
        Guid, 
        PagedAndSortedResultRequestDto, 
        CreateCertificateDefinitionDto, 
        UpdateCertificateDefinitionDto>
{
}
