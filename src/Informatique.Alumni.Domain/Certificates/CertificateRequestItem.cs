using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Certificates;

public class CertificateRequestItem : Entity<Guid>
{
    public Guid CertificateRequestId { get; private set; }
    public Guid CertificateDefinitionId { get; private set; }
    public Guid? QualificationId { get; private set; } // FK to Education record
    public CertificateLanguage Language { get; private set; }
    public decimal Fee { get; private set; }
    
    // Anti-fraud fields (per certificate item)
    public string? VerificationHash { get; private set; }
    public DateTime? GenerationDate { get; private set; }
    public string? QrCodeContent { get; private set; }

    private CertificateRequestItem() { }

    public CertificateRequestItem(
        Guid id,
        Guid certificateRequestId,
        Guid certificateDefinitionId,
        CertificateLanguage language,
        decimal fee,
        Guid? qualificationId = null)
        : base(id)
    {
        CertificateRequestId = Check.NotDefaultOrNull<Guid>(certificateRequestId, nameof(certificateRequestId));
        CertificateDefinitionId = Check.NotDefaultOrNull<Guid>(certificateDefinitionId, nameof(certificateDefinitionId));
        Language = language;
        Fee = fee;
        QualificationId = qualificationId;
    }

    public void MarkAsReady(string verificationHash, string qrCodeContent)
    {
        VerificationHash = Check.NotNullOrWhiteSpace(verificationHash, nameof(verificationHash), CertificateConsts.MaxHashLength);
        QrCodeContent = Check.NotNullOrWhiteSpace(qrCodeContent, nameof(qrCodeContent), CertificateConsts.MaxQrCodeLength);
        GenerationDate = DateTime.UtcNow;
    }
}
