using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Informatique.Alumni.Branches;
using Volo.Abp.Users;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Informatique.Alumni;
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
using Informatique.Alumni.Dashboard;
using Informatique.Alumni.Trips;
using Informatique.Alumni.Infrastructure.SMS; // Added for SMS Logs
using Informatique.Alumni.Infrastructure.Email; // Added for Email Logs
using Informatique.Alumni.Payment;
using Informatique.Alumni.Delivery;
using Informatique.Alumni.Communications;
using Informatique.Alumni.EntityFrameworkCore.Syndicates;
using Informatique.Alumni.EntityFrameworkCore.Membership;

namespace Informatique.Alumni.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class AlumniDbContext :
    AbpDbContext<AlumniDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<Branch> Branches { get; set; }
    public DbSet<CertificateDefinition> CertificateDefinitions { get; set; }
    public DbSet<CertificateRequest> CertificateRequests { get; set; }
    public DbSet<CertificateRequestItem> CertificateRequestItems { get; set; }
    public DbSet<CertificateRequestHistory> CertificateRequestHistory { get; set; }

    // Membership
    public DbSet<SubscriptionFee> SubscriptionFees { get; set; }
    public DbSet<AssociationRequest> AssociationRequests { get; set; }
    public DbSet<Informatique.Alumni.Membership.MembershipFeeConfig> MembershipFeeConfigs { get; set; }
    public DbSet<Informatique.Alumni.Membership.PaymentTransaction> PaymentTransactions { get; set; }

    // Profiles
    public DbSet<AlumniProfile> AlumniProfiles { get; set; }
    public DbSet<Experience> Experiences { get; set; }
    public DbSet<Education> Educations { get; set; }
    public DbSet<College> Colleges { get; set; }
    public DbSet<Major> Majors { get; set; }
    public DbSet<Nationality> Nationalities { get; set; } // Added for Reporting


    // Directory
    public DbSet<AlumniDirectoryCache> DirectoryCache { get; set; }

    // Gallery
    public DbSet<GalleryAlbum> GalleryAlbums { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }

    // Health
    public DbSet<MedicalPartner> MedicalPartners { get; set; }
    public DbSet<MedicalOffer> MedicalOffers { get; set; }

    // Magazine & Blog
    public DbSet<Informatique.Alumni.Magazine.Magazine> Magazines { get; set; } 
    public DbSet<MagazineIssue> MagazineIssues { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<PostComment> PostComments { get; set; }

    // Guidance
    public DbSet<GuidanceSessionRule> GuidanceSessionRules { get; set; }
    public DbSet<AdvisingRequest> AdvisingRequests { get; set; }
    
    // Benefits
    public DbSet<AcademicGrant> AcademicGrants { get; set; }
    public DbSet<CommercialDiscount> CommercialDiscounts { get; set; }
    public DbSet<DiscountCategory> DiscountCategories { get; set; }
    
    // Career
    public DbSet<CareerService> CareerServices { get; set; }
    public DbSet<CareerServiceType> CareerServiceTypes { get; set; }
    public DbSet<CareerServiceTimeslot> CareerServiceTimeslots { get; set; }
    public DbSet<AlumniCareerSubscription> AlumniCareerSubscriptions { get; set; }
    
    // Syndicates
    public DbSet<Syndicate> Syndicates { get; set; }
    public DbSet<SyndicateSubscription> SyndicateSubscriptions { get; set; }
    public DbSet<SyndicateDocument> SyndicateDocuments { get; set; }
    
    // Events
    public DbSet<Company> Companies { get; set; }
    public DbSet<ActivityType> ActivityTypes { get; set; } // New Entity
    public DbSet<ParticipationType> ParticipationTypes { get; set; }
    public DbSet<AssociationEvent> AssociationEvents { get; set; }
    public DbSet<EventAgendaItem> EventAgendaItems { get; set; }
    public DbSet<AlumniEventRegistration> AlumniEventRegistrations { get; set; }
    
    // Career & Job Board
    public DbSet<CurriculumVitae> CurriculumVitaes { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobApplication> JobApplications { get; set; }
    
    // CV Children
    public DbSet<CvEducation> CvEducations { get; set; }
    public DbSet<CvExperience> CvExperiences { get; set; }
    public DbSet<CvSkill> CvSkills { get; set; }
    public DbSet<CvLanguage> CvLanguages { get; set; }
    public DbSet<CvCertification> CvCertifications { get; set; }
    public DbSet<CvProject> CvProjects { get; set; }
    public DbSet<CvAward> CvAwards { get; set; }
    public DbSet<CvVolunteerWork> CvVolunteerWorks { get; set; }
    public DbSet<CvReference> CvReferences { get; set; }
    public DbSet<CvPublication> CvPublications { get; set; }
    public DbSet<CvInterest> CvInterests { get; set; }
    public DbSet<CvSocialLink> CvSocialLinks { get; set; }
    public DbSet<CvCourse> CvCourses { get; set; }
    public DbSet<CvPracticalTraining> CvPracticalTrainings { get; set; }
    
    // Dashboard & Trips
    public DbSet<DailyStats> DailyDashboardStats { get; set; }
    public DbSet<AlumniTrip> AlumniTrips { get; set; }
    public DbSet<TripRequest> TripRequests { get; set; }

    // Infrastructure
    public DbSet<SmsDeliveryLog> SmsLogs { get; set; }
    public DbSet<EmailDeliveryLog> EmailLogs { get; set; }

    // Payment
    public DbSet<Informatique.Alumni.Payment.PaymentTransaction> AppPaymentTransactions { get; set; }
    public DbSet<PaymentConfig> PaymentConfigs { get; set; }
    public DbSet<Informatique.Alumni.Payment.PaymentMethod> PaymentMethods { get; set; }

    // Delivery
    public DbSet<DeliveryProvider> DeliveryProviders { get; set; }
    public DbSet<ShipmentRequest> ShipmentRequests { get; set; }
    public DbSet<CommunicationLog> CommunicationLogs { get; set; }

    // Legacy SIS Simulation
    public DbSet<SisQualification> SisQualifications { get; set; }
    public DbSet<SisSemester> SisSemesters { get; set; }
    public DbSet<SisCourse> SisCourses { get; set; }
    public DbSet<SisExpectedGraduate> SisExpectedGraduates { get; set; }


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public virtual Guid? CurrentCollegeId => LazyServiceProvider?.LazyGetService<ICurrentUser>()?.GetCollegeId();

    public AlumniDbContext(DbContextOptions<AlumniDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();
        
        /* Configure your own tables/entities inside here */

        builder.Entity<Branch>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Branches", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Code).IsRequired().HasMaxLength(32);
        });

        builder.Entity<CertificateDefinition>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CertificateDefinitions", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(CertificateConsts.MaxNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(CertificateConsts.MaxNameLength);
            b.Property(x => x.Description).HasMaxLength(CertificateConsts.MaxDescriptionLength);
            b.Property(x => x.Fee).HasColumnType("decimal(18,2)");
        });

        builder.Entity<CertificateRequest>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CertificateRequests", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.CertificateRequestId).IsRequired();
            b.HasMany(x => x.History).WithOne().HasForeignKey(x => x.CertificateRequestId).IsRequired();
        });

        builder.Entity<CertificateRequestItem>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CertificateRequestItems", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<CertificateDefinition>().WithMany().HasForeignKey(x => x.CertificateDefinitionId).IsRequired();
            b.HasIndex(x => x.CertificateDefinitionId);
            b.HasIndex(x => x.CertificateRequestId);
        });
        
         builder.Entity<CertificateRequestHistory>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CertificateRequestHistories", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.CertificateRequestId);
        });


        // Membership
        builder.ApplyConfiguration(new AssociationRequestConfiguration());
        builder.ApplyConfiguration(new SubscriptionFeeConfiguration());
        builder.ApplyConfiguration(new RequestStatusHistoryConfiguration());
        builder.ApplyConfiguration(new MembershipFeeConfigConfiguration());

        builder.Entity<Informatique.Alumni.Membership.PaymentTransaction>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "PaymentTransactions", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ExternalTransactionId).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.RequestId);
        });

        // Profiles
        builder.Entity<AlumniProfile>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AlumniProfiles", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.UserId);
            b.HasOne<Nationality>().WithMany().HasForeignKey(x => x.NationalityId).IsRequired(false);
        });

        builder.Entity<Experience>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Experiences", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.AlumniProfileId);
        });

        builder.Entity<Education>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Educations", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.AlumniProfileId);
        });

        builder.Entity<Nationality>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Nationalities", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        });

        builder.Entity<College>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Colleges", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
        });

        builder.Entity<Major>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Majors", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.CollegeId);
        });

        // Directory
        builder.Entity<AlumniDirectoryCache>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AlumniDirectoryCache", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.College);
            b.HasIndex(x => x.Major);
            b.HasIndex(x => x.GraduationYear);
        });

        builder.Entity<GalleryAlbum>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "GalleryAlbums", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(GalleryConsts.MaxAlbumNameLength);
            b.Property(x => x.Description).HasMaxLength(GalleryConsts.MaxDescriptionLength);
            b.HasMany(p => p.MediaItems).WithOne().HasForeignKey(x => x.GalleryAlbumId).IsRequired();
        });

        builder.Entity<GalleryImage>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "GalleryImages", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.OriginalBlobName).HasMaxLength(256);
            b.Property(x => x.ThumbnailBlobName).HasMaxLength(256);
            b.HasIndex(x => x.GalleryAlbumId);
        });

        // Health
        builder.Entity<MedicalPartner>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "MedicalPartners", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Address).HasMaxLength(512);
            b.Property(x => x.ContactNumber).HasMaxLength(32);
            b.HasIndex(x => x.Type);
        });

        builder.Entity<MedicalOffer>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "MedicalOffers", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.DiscountCode).HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.HasIndex(x => x.MedicalPartnerId);
        });

        // Magazine & Blog
        builder.Entity<Informatique.Alumni.Magazine.Magazine>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Magazines", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(MagazineConsts.MaxTitleLength);
            b.Property(x => x.FileBlobName).HasMaxLength(256);
        });

        builder.Entity<MagazineIssue>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "MagazineIssues", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.PdfBlobName).HasMaxLength(256);
            b.Property(x => x.ThumbnailBlobName).HasMaxLength(256);
        });

        builder.Entity<BlogPost>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "BlogPosts", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.AuthorId);
            b.HasIndex(x => x.Category);
        });

        builder.Entity<PostComment>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "PostComments", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Content).HasMaxLength(2000);
            b.HasIndex(x => x.BlogPostId);
            b.HasIndex(x => x.AlumniId);
        });

        builder.Entity<GuidanceSessionRule>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "GuidanceSessionRules", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.BranchId);
        });

        builder.Entity<AdvisingRequest>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AdvisingRequests", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Subject).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.AdvisorId);
            b.HasIndex(x => x.AlumniId);
            b.HasIndex(x => new { x.AdvisorId, x.StartTime, x.EndTime });
        });

        builder.Entity<CareerService>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CareerServices", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(128);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(128);
            b.Property(x => x.Code).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(CareerConsts.MaxDescriptionLength);
        });

        builder.Entity<CareerServiceType>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CareerServiceTypes", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(128);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(128);
        });

        builder.Entity<AlumniCareerSubscription>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AlumniCareerSubscriptions", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.CareerServiceId, x.AlumniId }).IsUnique();
        });

        builder.ApplyConfiguration(new SyndicateConfiguration());
        builder.ApplyConfiguration(new SyndicateSubscriptionConfiguration());
        builder.ApplyConfiguration(new SyndicateDocumentConfiguration());

        builder.Entity<Company>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Companies", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(EventConsts.MaxCompanyNameLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(EventConsts.MaxCompanyNameLength);
            b.Property(x => x.WebsiteUrl).HasMaxLength(EventConsts.MaxWebsiteLength);
            
            b.HasIndex(x => x.NameAr).IsUnique();
            b.HasIndex(x => x.NameEn).IsUnique();
        });

        builder.Entity<ParticipationType>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "ParticipationTypes", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
            
            b.HasIndex(x => x.NameAr).IsUnique();
            b.HasIndex(x => x.NameEn).IsUnique();
        });

        builder.Entity<ActivityType>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "ActivityTypes", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
            b.HasIndex(x => x.NameAr).IsUnique();
            b.HasIndex(x => x.NameEn).IsUnique();
        });

        builder.Entity<AssociationEvent>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Events", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(32);
            b.Property(x => x.Location).HasMaxLength(EventConsts.MaxLocationLength);
            b.Property(x => x.FeeAmount).HasColumnType("decimal(18,2)");
            
            b.HasMany(x => x.Timeslots).WithOne().HasForeignKey(x => x.EventId).IsRequired();
            b.HasMany(x => x.ParticipatingCompanies).WithOne().HasForeignKey(x => x.EventId).IsRequired();
            
            b.HasOne(x => x.ActivityType).WithMany().HasForeignKey(x => x.ActivityTypeId).IsRequired(false);
        });

        builder.Entity<EventTimeslot>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "EventTimeslots", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<EventParticipatingCompany>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "EventParticipatingCompanies", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne(x => x.ParticipationType).WithMany().HasForeignKey(x => x.ParticipationTypeId).IsRequired();
        });

        builder.Entity<AlumniEventRegistration>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AlumniEventRegistrations", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TicketCode).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.TicketCode).IsUnique();
            b.HasIndex(x => new { x.AlumniId, x.EventId }).IsUnique();
        });

        // CV & Jobs
        builder.Entity<CurriculumVitae>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CurriculumVitaes", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.AlumniId).IsUnique();
            
            // 14 Cascade Deletes
            b.HasMany(x => x.Educations).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Experiences).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Skills).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Languages).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Certifications).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Projects).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Awards).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.VolunteerWorks).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.References).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Publications).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Interests).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.SocialLinks).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.Courses).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(x => x.PracticalTrainings).WithOne().HasForeignKey(x => x.CurriculumVitaeId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Job>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "Jobs", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
        });

        builder.Entity<JobApplication>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "JobApplications", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.JobId, x.AlumniId }).IsUnique();
        });

        // CV Child Tables
        ConfigureCvChild<CvEducation>(builder, "CvEducations");
        ConfigureCvChild<CvExperience>(builder, "CvExperiences");
        ConfigureCvChild<CvSkill>(builder, "CvSkills");
        ConfigureCvChild<CvLanguage>(builder, "CvLanguages");
        ConfigureCvChild<CvCertification>(builder, "CvCertifications");
        ConfigureCvChild<CvProject>(builder, "CvProjects");
        ConfigureCvChild<CvAward>(builder, "CvAwards");
        ConfigureCvChild<CvVolunteerWork>(builder, "CvVolunteerWorks");
        ConfigureCvChild<CvReference>(builder, "CvReferences");
        ConfigureCvChild<CvPublication>(builder, "CvPublications");
        ConfigureCvChild<CvInterest>(builder, "CvInterests");
        ConfigureCvChild<CvSocialLink>(builder, "CvSocialLinks");
        ConfigureCvChild<CvPracticalTraining>(builder, "CvPracticalTrainings");

        builder.Entity<AcademicGrant>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AcademicGrants", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(BenefitConsts.MaxTitleLength); // Assuming MaxTitleLength is appropriate
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(BenefitConsts.MaxTitleLength);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50); // Hardcoded or find constants?
            // b.Property(x => x.Amount).HasColumnType("decimal(18,2)"); // Removed as it doesn't exist
            b.Property(x => x.Percentage);
        });

        builder.Entity<CommercialDiscount>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CommercialDiscounts", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ProviderName).IsRequired().HasMaxLength(BenefitConsts.MaxProviderLength);
            b.Property(x => x.Title).IsRequired().HasMaxLength(BenefitConsts.MaxTitleLength);
            b.Property(x => x.Description).IsRequired().HasMaxLength(BenefitConsts.MaxDescriptionLength);
            b.Property(x => x.DiscountPercentage).HasColumnType("decimal(5,2)");
            b.Property(x => x.PromoCode).HasMaxLength(BenefitConsts.MaxPromoCodeLength);
        });

        builder.Entity<DiscountCategory>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "DiscountCategories", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(BenefitConsts.MaxTitleLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(BenefitConsts.MaxTitleLength);
            b.Property(x => x.LogoUrl).HasMaxLength(256);
        });

        // builder.Entity<AcademicGrant>().HasQueryFilter(x => x.ValidUntil > DateTime.Now);
        builder.Entity<CommercialDiscount>().HasQueryFilter(x => x.ValidUntil > DateTime.Now);

        builder.Entity<Branch>().HasQueryFilter(b => !CurrentCollegeId.HasValue || b.Id == CurrentCollegeId);
        // builder.Entity<IdentityUser>().HasQueryFilter(u => !CurrentCollegeId.HasValue || EF.Property<Guid?>(u, "CollegeId") == CurrentCollegeId);

        // Dashboard & Trips
        builder.Entity<DailyStats>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "DailyStats", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Date).IsUnique();
        });

        builder.Entity<AlumniTrip>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AlumniTrips", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.PricePerPerson).HasColumnType("decimal(18,2)");
        });

        builder.Entity<TripRequest>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "TripRequests", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.HasIndex(x => new { x.TripId, x.AlumniId });
        });

        // Infrastructure - SMS
        builder.Entity<SmsDeliveryLog>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "SmsLogs", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Timestamp);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Timestamp);
            b.HasIndex(x => x.Recipient);
        });

        // Infrastructure - Email
        builder.Entity<EmailDeliveryLog>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "EmailLogs", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.Timestamp);
            b.HasIndex(x => x.Recipient);
        });

        // Payment Module
        builder.Entity<Informatique.Alumni.Payment.PaymentTransaction>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "AppPaymentTransactions", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            // Immutable ledger: Updates generally discouraged, logic enforcement in Domain
            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.GatewayTransactionId);
        });

        builder.Entity<PaymentConfig>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "PaymentConfigs", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
        });

        // Delivery Module
        builder.Entity<DeliveryProvider>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "DeliveryProviders", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<ShipmentRequest>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "ShipmentRequests", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Fee).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.TrackingNumber);
        });

        // Communication
        builder.Entity<CommunicationLog>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "CommunicationLogs", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Subject).HasMaxLength(256); // Email Subject Limit
            b.HasIndex(x => x.SenderId);
            b.HasIndex(x => x.RecipientId);
            b.HasIndex(x => x.CreationTime);
        });

        // Legacy SIS Simulation
        builder.Entity<SisQualification>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "SisQualifications", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Semesters).WithOne().HasForeignKey(x => x.QualificationId).IsRequired();
            b.HasIndex(x => x.StudentId);
        });

        builder.Entity<SisSemester>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "SisSemesters", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Courses).WithOne().HasForeignKey(x => x.SemesterId).IsRequired();
        });

        builder.Entity<SisCourse>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "SisCourses", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<SisExpectedGraduate>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + "SisExpectedGraduates", AlumniConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => x.StudentId);
        });
    }

    private void ConfigureCvChild<T>(ModelBuilder builder, string tableName) where T : class
    {
        builder.Entity<T>(b =>
        {
            b.ToTable(AlumniConsts.DbTablePrefix + tableName, AlumniConsts.DbSchema);
            b.ConfigureByConvention();
        });
    }

    protected override bool ShouldFilterEntity<TEntity>(Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
    {
        return base.ShouldFilterEntity<TEntity>(entityType);
    }
}
