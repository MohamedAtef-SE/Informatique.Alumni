using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Certificates;

public class CertificateAvailabilityDto
{
    public bool IsEligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public List<CertificateDefinitionDto> Items { get; set; } = new();
}
