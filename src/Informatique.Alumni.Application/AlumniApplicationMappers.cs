using System.Collections.Generic;
using Riok.Mapperly.Abstractions;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Certificates;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Directory;
using Informatique.Alumni.Gallery;
using Informatique.Alumni.Health;
using Informatique.Alumni.Magazine;
using Informatique.Alumni.Guidance;
using Informatique.Alumni.Benefits;
using Informatique.Alumni.Career;
using Informatique.Alumni.Syndicates;
using Informatique.Alumni.Events;
using Informatique.Alumni.Payment;
using Informatique.Alumni.Delivery;
using Informatique.Alumni.Dashboard;
using Informatique.Alumni.Trips;

using Volo.Abp.DependencyInjection;

namespace Informatique.Alumni;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AlumniApplicationMappers : ITransientDependency
{
    public partial BranchDto MapToDto(Branch branch);
    public partial List<BranchDto> MapToDtos(List<Branch> branches);

    public partial CertificateDefinitionDto MapToDto(CertificateDefinition entity);
    public partial List<CertificateDefinitionDto> MapToDtos(IEnumerable<CertificateDefinition> entities);

    public partial CertificateRequestDto MapToDto(CertificateRequest entity);
    public partial List<CertificateRequestDto> MapToDtos(IEnumerable<CertificateRequest> entities);
    
    public partial CertificateRequestItemDto MapToDto(CertificateRequestItem entity);
    public partial List<CertificateRequestItemDto> MapToDtos(IEnumerable<CertificateRequestItem> entities);
    
    public partial CertificateRequestHistoryDto MapToDto(CertificateRequestHistory entity);
    public partial List<CertificateRequestHistoryDto> MapToDtos(IEnumerable<CertificateRequestHistory> entities);

    public partial SubscriptionFeeDto MapToDto(SubscriptionFee entity);
    public partial List<SubscriptionFeeDto> MapToDtos(IEnumerable<SubscriptionFee> entities);

    [MapperIgnoreTarget(nameof(AssociationRequestDto.SubscriptionFeeName))]
    [MapperIgnoreTarget(nameof(AssociationRequestDto.AlumniName))]
    [MapperIgnoreTarget(nameof(AssociationRequestDto.AlumniNationalId))]
    [MapperIgnoreTarget(nameof(AssociationRequestDto.AlumniPhotoUrl))]
    [MapperIgnoreTarget(nameof(AssociationRequestDto.CollegeName))]
    [MapperIgnoreTarget(nameof(AssociationRequestDto.GraduationYear))]
    [MapperIgnoreTarget(nameof(AssociationRequestDto.EligibilityChecks))]
    [MapperIgnoreTarget(nameof(AssociationRequestDto.EligibilitySummary))]
    public partial AssociationRequestDto MapToDto(AssociationRequest entity);
    public partial List<AssociationRequestDto> MapToDtos(IEnumerable<AssociationRequest> entities);

    public partial AlumniProfileDto MapToDto(AlumniProfile entity);
    public partial ExperienceDto MapToDto(Experience entity);
    public partial EducationDto MapToDto(Education entity);

    // Removed MapToEntity methods - use entity Update/business methods instead
    // - AlumniProfile.UpdateBasicInfo()
    // - Experience/Education added via AlumniProfile.AddExperience/AddEducation

    public partial AlumniDirectoryDto MapToDto(AlumniDirectoryCache entity);
    public partial List<AlumniDirectoryDto> MapToDtos(IEnumerable<AlumniDirectoryCache> entities);

    [MapProperty(nameof(GalleryImage.OriginalBlobName), nameof(GalleryImageDto.OriginalUrl))]
    [MapProperty(nameof(GalleryImage.ThumbnailBlobName), nameof(GalleryImageDto.ThumbnailUrl))]
    public partial GalleryImageDto MapToDto(GalleryImage entity);
    public partial List<GalleryImageDto> MapToDtos(IEnumerable<GalleryImage> entities);

    public partial GalleryAlbumDto MapToDto(GalleryAlbum entity);
    public partial List<GalleryAlbumDto> MapToDtos(IEnumerable<GalleryAlbum> entities);

    public partial MedicalPartnerDto MapToDto(MedicalPartner entity);
    public partial List<MedicalPartnerDto> MapToDtos(IEnumerable<MedicalPartner> entities);
    public partial MedicalOfferDto MapToDto(MedicalOffer entity);
    
    public partial void MapToEntity(CreateUpdateMedicalPartnerDto dto, MedicalPartner entity);
    public partial void MapToEntity(CreateUpdateMedicalOfferDto dto, MedicalOffer entity);

    [MapProperty(nameof(MagazineIssue.PdfBlobName), nameof(MagazineIssueDto.PdfUrl))]
    [MapProperty(nameof(MagazineIssue.ThumbnailBlobName), nameof(MagazineIssueDto.ThumbnailUrl))]
    public partial MagazineIssueDto MapToDto(MagazineIssue entity);
    public partial List<MagazineIssueDto> MapToDtos(IEnumerable<MagazineIssue> entities);

    [MapProperty(nameof(BlogPost.CoverImageBlobName), nameof(BlogPostDto.CoverImageUrl))]
    [MapperIgnoreTarget(nameof(BlogPostDto.AuthorName))]
    public partial BlogPostDto MapToDto(BlogPost entity);
    public partial List<BlogPostDto> MapToDtos(IEnumerable<BlogPost> entities);

    public partial PostCommentDto MapToDto(PostComment entity);
    public partial List<PostCommentDto> MapToDtos(IEnumerable<PostComment> entities);

    public partial AdvisingRequestDto MapToDto(AdvisingRequest entity);
    public partial List<AdvisingRequestDto> MapToDtos(IEnumerable<AdvisingRequest> entities);

    public partial GuidanceSessionRuleDto MapToDto(GuidanceSessionRule entity);
    public partial List<GuidanceSessionRuleDto> MapToDtos(IEnumerable<GuidanceSessionRule> entities);



    public partial AcademicGrantDto MapToDto(AcademicGrant entity);
    public partial List<AcademicGrantDto> MapToDtos(IEnumerable<AcademicGrant> entities);
    public partial void MapToEntity(CreateUpdateAcademicGrantDto dto, AcademicGrant entity);

    public partial AcademicDiscountDto MapToDto(AcademicDiscount entity);
    public partial List<AcademicDiscountDto> MapToDtos(IEnumerable<AcademicDiscount> entities);

    public partial CommercialDiscountDto MapToDto(CommercialDiscount entity);
    public partial List<CommercialDiscountDto> MapToDtos(IEnumerable<CommercialDiscount> entities);
    public partial void MapToEntity(CreateUpdateCommercialDiscountDto dto, CommercialDiscount entity);

    public partial CareerServiceTypeDto MapToDto(CareerServiceType entity);
    public partial List<CareerServiceTypeDto> MapToDtos(IEnumerable<CareerServiceType> entities);
    
    public partial CareerLookupItemDto MapToLookupItemDto(CareerServiceType entity);
    [MapProperty(nameof(Branch.Name), nameof(CareerLookupItemDto.NameEn))]
    [MapProperty(nameof(Branch.Name), nameof(CareerLookupItemDto.NameAr))]
    public partial CareerLookupItemDto MapToLookupItemDto(Branch entity);

    public partial CareerServiceDto MapToDto(CareerService entity);
    public partial List<CareerServiceDto> MapToDtos(IEnumerable<CareerService> entities);

    public partial CareerServiceTimeslotDto MapToDto(CareerServiceTimeslot entity);
    public partial List<CareerServiceTimeslotDto> MapToDtos(IEnumerable<CareerServiceTimeslot> entities);

    public partial AlumniCareerSubscriptionDto MapToDto(AlumniCareerSubscription entity);
    
    // Mapperly might struggle with private constructor. 
    // I should create Timeslot manually in AppService or expose a constructor for Mapper?
    // Better: Handle Timeslot creation in AppService loop for now or assume Mapperly can standard map if I add a constructor or configuration.
    // I will stick to adding MapToDto for now which is critical for Reading.
    // Writing (CreateActivity) might need manual code if Mapper fails on Entity constructor.
    // Let's add MapToDto first.

    public partial SyndicateDto MapToDto(Syndicate entity);
    public partial List<SyndicateDto> MapToDtos(IEnumerable<Syndicate> entities);
    
    public partial SyndicateSubscriptionDto MapToDto(SyndicateSubscription entity);
    public partial List<SyndicateSubscriptionDto> MapToDtos(IEnumerable<SyndicateSubscription> entities);
    
    public partial SyndicateDocumentDto MapToDto(SyndicateDocument entity);

    public partial SyndicateRequirementDto MapToDto(SyndicateRequirement entity);
    public partial List<SyndicateRequirementDto> MapToDtos(IEnumerable<SyndicateRequirement> entities);

    public partial AssociationEventDto MapToDto(AssociationEvent entity);
    public partial List<AssociationEventDto> MapToDtos(IEnumerable<AssociationEvent> entities);
    
    public partial EventTimeslotDto MapToDto(EventTimeslot entity);
    public partial EventParticipatingCompanyDto MapToDto(EventParticipatingCompany entity);


    
    public partial AlumniEventRegistrationDto MapToDto(AlumniEventRegistration entity);
    public partial List<AlumniEventRegistrationDto> MapToDtos(IEnumerable<AlumniEventRegistration> entities);

    public partial CompanyDto MapToDto(Company entity);
    public partial List<CompanyDto> MapToDtos(IEnumerable<Company> entities);
    public partial void MapToEntity(CompanyDto dto, Company entity);
    
    public partial ParticipationTypeDto MapToDto(ParticipationType entity);
    public partial List<ParticipationTypeDto> MapToDtos(IEnumerable<ParticipationType> entities);

    public partial ActivityTypeDto MapToDto(ActivityType entity);
    public partial List<ActivityTypeDto> MapToDtos(IEnumerable<ActivityType> entities);

    // CV & Job Board
    public partial CurriculumVitaeDto MapToDto(CurriculumVitae entity);
    public partial List<CurriculumVitaeDto> MapToDtos(IEnumerable<CurriculumVitae> entities);
    public partial void MapToEntity(CurriculumVitaeDto dto, CurriculumVitae entity);

    public partial JobDto MapToDto(Job entity);
    public partial List<JobDto> MapToDtos(IEnumerable<Job> entities);
    // Removed MapToEntity - use entity.Update() method instead

    public partial JobApplicationDto MapToDto(JobApplication entity);
    public partial List<JobApplicationDto> MapToDtos(IEnumerable<JobApplication> entities);

    // Child Collections
    public partial CvEducationDto MapToDto(CvEducation entity);
    public partial CvExperienceDto MapToDto(CvExperience entity);
    public partial CvSkillDto MapToDto(CvSkill entity);
    public partial CvLanguageDto MapToDto(CvLanguage entity);
    public partial CvCertificationDto MapToDto(CvCertification entity);
    public partial CvProjectDto MapToDto(CvProject entity);
    public partial CvAwardDto MapToDto(CvAward entity);
    public partial CvVolunteerWorkDto MapToDto(CvVolunteerWork entity);
    public partial CvReferenceDto MapToDto(CvReference entity);
    public partial CvPublicationDto MapToDto(CvPublication entity);
    public partial CvInterestDto MapToDto(CvInterest entity);
    public partial CvSocialLinkDto MapToDto(CvSocialLink entity);
    public partial CvCourseDto MapToDto(CvCourse entity);
    public partial CvPracticalTrainingDto MapToDto(CvPracticalTraining entity);

    // Dashboard & Trips
    public partial DailyStatsDto MapToDto(DailyStats entity);
    public partial AlumniTripDto MapToDto(AlumniTrip entity);
    public partial List<AlumniTripDto> MapToDtos(IEnumerable<AlumniTrip> entities);
    public partial TripRequestDto MapToDto(TripRequest entity);

    // Payment
    public partial PaymentTransactionDto MapToDto(Informatique.Alumni.Payment.PaymentTransaction entity);

    // Delivery
    public partial ShipmentRequestDto MapToDto(ShipmentRequest entity);
}
