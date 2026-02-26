using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Certificates;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Syndicates;
using Informatique.Alumni.Trips;
using Informatique.Alumni.Gallery;
using Informatique.Alumni.Health;
using Informatique.Alumni.Benefits;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Informatique.Alumni.Admin;

// ================== Certificate Admin ==================

public class CertificateRequestAdminDto : EntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public CertificateRequestStatus Status { get; set; }
    public Certificates.DeliveryMethod DeliveryMethod { get; set; }
    public decimal TotalItemFees { get; set; }
    public decimal DeliveryFee { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CertificateDefinitionAdminDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public DegreeType DegreeType { get; set; }
    public bool IsActive { get; set; }
}

public class CertificateAdminGetListInput : PagedAndSortedResultRequestDto
{
    public CertificateRequestStatus? StatusFilter { get; set; }
}

public interface ICertificateAdminAppService : IApplicationService
{
    Task<PagedResultDto<CertificateRequestAdminDto>> GetRequestsAsync(CertificateAdminGetListInput input);
    Task MoveToProcessingAsync(Guid id);
    Task MarkAsReadyForPickupAsync(Guid id);
    Task MarkAsOutForDeliveryAsync(Guid id);
    Task DeliverAsync(Guid id);
    Task RejectAsync(Guid id, string reason);
}

// ================== Membership Admin ==================

public class MembershipRequestAdminDto : EntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public MembershipRequestStatus Status { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? RejectionReason { get; set; }
    public decimal DeliveryFee { get; set; }
    public Membership.DeliveryMethod DeliveryMethod { get; set; }
    public DateTime ValidityStartDate { get; set; }
    public DateTime ValidityEndDate { get; set; }
    public DateTime CreationTime { get; set; }
}

public class MembershipAdminGetListInput : PagedAndSortedResultRequestDto
{
    public MembershipRequestStatus? StatusFilter { get; set; }
}

public interface IMembershipAdminAppService : IApplicationService
{
    Task<PagedResultDto<MembershipRequestAdminDto>> GetRequestsAsync(MembershipAdminGetListInput input);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id, string reason);
    Task MarkAsPaidAsync(Guid id);
}

// ================== Syndicate Admin ==================

public class SyndicateSubscriptionAdminDto : EntityDto<Guid>
{
    public Guid AlumniId { get; set; }
    public string AlumniName { get; set; } = string.Empty;
    public string AlumniNationalId { get; set; } = string.Empty;
    public string AlumniMobile { get; set; } = string.Empty;
    public Guid SyndicateId { get; set; }
    public string SyndicateName { get; set; } = string.Empty;
    public SyndicateStatus Status { get; set; }
    public decimal FeeAmount { get; set; }
    public Syndicates.PaymentStatus PaymentStatus { get; set; }
    public Membership.DeliveryMethod DeliveryMethod { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreationTime { get; set; }
    public List<SyndicateDocumentDto> Documents { get; set; } = new();
}

public class SyndicateAdminGetListInput : PagedAndSortedResultRequestDto
{
    public SyndicateStatus? StatusFilter { get; set; }
}

public interface ISyndicateAdminAppService : IApplicationService
{
    Task<PagedResultDto<SyndicateSubscriptionAdminDto>> GetSubscriptionsAsync(SyndicateAdminGetListInput input);
    Task MarkAsInProgressAsync(Guid id);
    Task MarkAsReadyForPickupAsync(Guid id);
    Task MarkAsReceivedAsync(Guid id);
    Task RejectAsync(Guid id, string reason);
    Task<IRemoteStreamContent> GetDocumentAsync(Guid subscriptionId, Guid documentId);
}

// ================== Trip Admin ==================

public class TripAdminDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public TripType TripType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int RequestCount { get; set; }
    public DateTime CreationTime { get; set; }
}

public class TripRequestAdminDto : EntityDto<Guid>
{
    public Guid TripId { get; set; }
    public Guid AlumniId { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalAmount { get; set; }
    public TripRequestStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}

public class TripAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}

public interface ITripAdminAppService : IApplicationService
{
    Task<PagedResultDto<TripAdminDto>> GetTripsAsync(TripAdminGetListInput input);
    Task<PagedResultDto<TripRequestAdminDto>> GetRequestsAsync(Guid tripId, PagedAndSortedResultRequestDto input);
    Task ApproveRequestAsync(Guid requestId);
    Task RejectRequestAsync(Guid requestId);
    Task ActivateTripAsync(Guid id);
    Task DeactivateTripAsync(Guid id);
}

// ================== Gallery Admin ==================

public class GalleryAlbumAdminDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public int MediaItemCount { get; set; }
    public DateTime CreationTime { get; set; }
}

public interface IGalleryAdminAppService : IApplicationService
{
    Task<PagedResultDto<GalleryAlbumAdminDto>> GetAlbumsAsync(PagedAndSortedResultRequestDto input);
    Task DeleteAlbumAsync(Guid id);
    Task DeleteMediaItemAsync(Guid albumId, Guid mediaItemId);
}

// ================== Healthcare Admin ==================

public class MedicalPartnerAdminDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public MedicalPartnerType Type { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int OfferCount { get; set; }
    public DateTime CreationTime { get; set; }
}

public interface IHealthcareAdminAppService : IApplicationService
{
    Task<PagedResultDto<MedicalPartnerAdminDto>> GetPartnersAsync(PagedAndSortedResultRequestDto input);
    Task TogglePartnerActiveAsync(Guid id);
}

// ================== Benefits Admin ==================

public class AcademicGrantAdminDto : EntityDto<Guid>
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Percentage { get; set; }
}

public class CommercialDiscountAdminDto : EntityDto<Guid>
{
    public string ProviderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public DateTime ValidUntil { get; set; }
}

public interface IBenefitsAdminAppService : IApplicationService
{
    Task<PagedResultDto<AcademicGrantAdminDto>> GetGrantsAsync(PagedAndSortedResultRequestDto input);
    Task<PagedResultDto<CommercialDiscountAdminDto>> GetDiscountsAsync(PagedAndSortedResultRequestDto input);
    Task DeleteGrantAsync(Guid id);
    Task DeleteDiscountAsync(Guid id);
}
