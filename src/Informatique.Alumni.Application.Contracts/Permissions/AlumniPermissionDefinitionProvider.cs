using Informatique.Alumni.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Informatique.Alumni.Permissions;

public class AlumniPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(AlumniPermissions.GroupName);

        var branchPermission = myGroup.AddPermission(AlumniPermissions.Branches.Default, L("Permission:Branches"));
        branchPermission.AddChild(AlumniPermissions.Branches.Create, L("Permission:Create"));
        branchPermission.AddChild(AlumniPermissions.Branches.Edit, L("Permission:Update"));
        branchPermission.AddChild(AlumniPermissions.Branches.Delete, L("Permission:Delete"));

        var userPermission = myGroup.AddPermission(AlumniPermissions.Users.CreateAlumni, L("Permission:CreateAlumni"));
        myGroup.AddPermission(AlumniPermissions.Users.Manage, L("Permission:ManageUsers"));
        myGroup.AddPermission(AlumniPermissions.Users.SystemUsersReport, L("Permission:SystemUsersReport"));
        myGroup.AddPermission(AlumniPermissions.Users.LoginAuditReport, L("Permission:LoginAuditReport"));

        var certificatePermission = myGroup.AddPermission(AlumniPermissions.Certificates.Default, L("Permission:Certificates"));
        certificatePermission.AddChild(AlumniPermissions.Certificates.ManageDefinitions, L("Permission:ManageDefinitions"));
        certificatePermission.AddChild(AlumniPermissions.Certificates.Request, L("Permission:RequestCertificate"));
        certificatePermission.AddChild(AlumniPermissions.Certificates.Process, L("Permission:ProcessRequests"));
        certificatePermission.AddChild(AlumniPermissions.Certificates.Verify, L("Permission:VerifyCertificate"));

        var membershipPermission = myGroup.AddPermission(AlumniPermissions.Membership.Default, L("Permission:Membership"));
        membershipPermission.AddChild(AlumniPermissions.Membership.ManageFees, L("Permission:ManageFees"));
        membershipPermission.AddChild(AlumniPermissions.Membership.Request, L("Permission:RequestMembership"));
        membershipPermission.AddChild(AlumniPermissions.Membership.Process, L("Permission:ProcessMembership"));
        membershipPermission.AddChild(AlumniPermissions.Membership.GlobalView, L("Permission:GlobalView"));

        var profilePermission = myGroup.AddPermission(AlumniPermissions.Profiles.Default, L("Permission:Profiles"));
        profilePermission.AddChild(AlumniPermissions.Profiles.Manage, L("Permission:ManageProfiles"));
        profilePermission.AddChild(AlumniPermissions.Profiles.ViewAll, L("Permission:ViewAllProfiles"));
        profilePermission.AddChild(AlumniPermissions.Profiles.Search, L("Permission:SearchProfiles"));
        profilePermission.AddChild(AlumniPermissions.Profiles.Edit, L("Permission:EditProfile"));

        var directoryPermission = myGroup.AddPermission(AlumniPermissions.Directory.Default, L("Permission:Directory"));
        directoryPermission.AddChild(AlumniPermissions.Directory.Search, L("Permission:SearchDirectory"));
        directoryPermission.AddChild(AlumniPermissions.Directory.ManageCache, L("Permission:ManageDirectoryCache"));

        var reportingPermission = myGroup.AddPermission(AlumniPermissions.Reporting.Default, L("Permission:Reporting"));
        reportingPermission.AddChild(AlumniPermissions.Reporting.BasicReport, L("Permission:BasicReport"));
        reportingPermission.AddChild(AlumniPermissions.Reporting.GraduatesReport, L("Permission:GraduatesReport"));

        var communicationPermission = myGroup.AddPermission(AlumniPermissions.Communication.Default, L("Permission:Communication"));
        communicationPermission.AddChild(AlumniPermissions.Communication.SendMassMessage, L("Permission:SendMassMessage"));

        var galleryPermission = myGroup.AddPermission(AlumniPermissions.Gallery.Default, L("Permission:Gallery"));
        galleryPermission.AddChild(AlumniPermissions.Gallery.Upload, L("Permission:UploadGallery"));
        galleryPermission.AddChild(AlumniPermissions.Gallery.Delete, L("Permission:DeleteGallery"));

        var healthPermission = myGroup.AddPermission(AlumniPermissions.Health.Default, L("Permission:Health"));
        healthPermission.AddChild(AlumniPermissions.Health.Manage, L("Permission:ManageHealth"));
        healthPermission.AddChild(AlumniPermissions.Health.ViewOffers, L("Permission:ViewHealthOffers"));

        var magazinePermission = myGroup.AddPermission(AlumniPermissions.Magazine.Default, L("Permission:Magazine"));
        magazinePermission.AddChild(AlumniPermissions.Magazine.ManageIssues, L("Permission:ManageMagazineIssues"));
        magazinePermission.AddChild(AlumniPermissions.Magazine.ManagePosts, L("Permission:ManageBlogPosts"));
        magazinePermission.AddChild(AlumniPermissions.Magazine.ApproveComments, L("Permission:ApproveComments"));

        var guidancePermission = myGroup.AddPermission(AlumniPermissions.Guidance.Default, L("Permission:Guidance"));
        guidancePermission.AddChild(AlumniPermissions.Guidance.ManageAvailability, L("Permission:ManageAvailability"));
        guidancePermission.AddChild(AlumniPermissions.Guidance.BookSession, L("Permission:BookSession"));
        guidancePermission.AddChild(AlumniPermissions.Guidance.ManageRequests, L("Permission:ManageRequests"));

        var benefitsPermission = myGroup.AddPermission(AlumniPermissions.Benefits.Default, L("Permission:Benefits"));
        benefitsPermission.AddChild(AlumniPermissions.Benefits.Manage, L("Permission:ManageBenefits"));
        benefitsPermission.AddChild(AlumniPermissions.Benefits.View, L("Permission:ViewBenefits"));

        var careerPermission = myGroup.AddPermission(AlumniPermissions.Career.Default, L("Permission:Career"));
        careerPermission.AddChild(AlumniPermissions.Career.Manage, L("Permission:ManageCareer"));
        careerPermission.AddChild(AlumniPermissions.Career.Register, L("Permission:RegisterCareer"));

        var syndicatePermission = myGroup.AddPermission(AlumniPermissions.Syndicates.Default, L("Permission:Syndicates"));
        syndicatePermission.AddChild(AlumniPermissions.Syndicates.Manage, L("Permission:ManageSyndicates"));
        syndicatePermission.AddChild(AlumniPermissions.Syndicates.Apply, L("Permission:ApplySyndicate"));

        var eventPermission = myGroup.AddPermission(AlumniPermissions.Events.Default, L("Permission:Events"));
        eventPermission.AddChild(AlumniPermissions.Events.Manage, L("Permission:ManageEvents"));
        eventPermission.AddChild(AlumniPermissions.Events.Register, L("Permission:RegisterEvent"));
        eventPermission.AddChild(AlumniPermissions.Events.VerifyTicket, L("Permission:VerifyTicket"));

        var companyPermission = myGroup.AddPermission(AlumniPermissions.Companies.Default, L("Permission:Companies"));
        companyPermission.AddChild(AlumniPermissions.Companies.Manage, L("Permission:ManageCompanies"));

        var careersPermission = myGroup.AddPermission(AlumniPermissions.Careers.Default, L("Permission:Careers"));
        careersPermission.AddChild(AlumniPermissions.Careers.JobManage, L("Permission:ManageJobs"));
        careersPermission.AddChild(AlumniPermissions.Careers.JobApply, L("Permission:ApplyForJobs"));
        careersPermission.AddChild(AlumniPermissions.Careers.CvAudit, L("Permission:AuditCVs"));
        careersPermission.AddChild(AlumniPermissions.Careers.CvManage, L("Permission:ManageOwnCV"));

        var dashboardPermission = myGroup.AddPermission(AlumniPermissions.Dashboard.Default, L("Permission:Dashboard"));
        dashboardPermission.AddChild(AlumniPermissions.Dashboard.ViewStatistics, L("Permission:ViewStatistics"));
        dashboardPermission.AddChild(AlumniPermissions.Dashboard.ViewFinancials, L("Permission:ViewFinancials"));

        var tripsPermission = myGroup.AddPermission(AlumniPermissions.Trips.Default, L("Permission:Trips"));
        tripsPermission.AddChild(AlumniPermissions.Trips.Manage, L("Permission:ManageTrips"));
        tripsPermission.AddChild(AlumniPermissions.Trips.Request, L("Permission:RequestTrip"));

        // Admin Module
        var adminPermission = myGroup.AddPermission(AlumniPermissions.Admin.Default, L("Permission:Admin"));
        adminPermission.AddChild(AlumniPermissions.Admin.AlumniManage, L("Permission:AdminAlumniManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.AlumniApprove, L("Permission:AdminAlumniApprove"));
        adminPermission.AddChild(AlumniPermissions.Admin.JobModerate, L("Permission:AdminJobModerate"));
        adminPermission.AddChild(AlumniPermissions.Admin.EventManage, L("Permission:AdminEventManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.Dashboard, L("Permission:AdminDashboard"));
        adminPermission.AddChild(AlumniPermissions.Admin.ContentModerate, L("Permission:AdminContentModerate"));
        adminPermission.AddChild(AlumniPermissions.Admin.GuidanceManage, L("Permission:AdminGuidanceManage"));

        // Phase 2
        adminPermission.AddChild(AlumniPermissions.Admin.CertificateManage, L("Permission:AdminCertificateManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.MembershipManage, L("Permission:AdminMembershipManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.SyndicateManage, L("Permission:AdminSyndicateManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.TripManage, L("Permission:AdminTripManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.GalleryManage, L("Permission:AdminGalleryManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.HealthcareManage, L("Permission:AdminHealthcareManage"));
        adminPermission.AddChild(AlumniPermissions.Admin.BenefitsManage, L("Permission:AdminBenefitsManage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AlumniResource>(name);
    }
}
