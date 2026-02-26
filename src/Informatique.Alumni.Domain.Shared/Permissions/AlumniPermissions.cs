namespace Informatique.Alumni.Permissions;

public static class AlumniPermissions
{
    public const string GroupName = "Alumni";


    
    public static class Branches
    {
        public const string Default = GroupName + ".Branches";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class Users
    {
        public const string CreateAlumni = GroupName + ".Users.CreateAlumni";
        public const string Manage = GroupName + ".Users.Manage";
        public const string SystemUsersReport = GroupName + ".Reports.SystemUsers";
        public const string LoginAuditReport = GroupName + ".Reports.LoginAudit";
    }

    public static class Certificates
    {
        public const string Default = GroupName + ".Certificates";
        public const string ManageDefinitions = Default + ".ManageDefinitions";
        public const string Request = Default + ".Request";
        public const string Process = Default + ".Process";
        public const string Verify = Default + ".Verify";
        public const string Reports = Default + ".Reports";
    }

    public static class Membership
    {
        public const string Default = GroupName + ".Membership";
        public const string ManageFees = Default + ".ManageFees";
        public const string Request = Default + ".Request";
        public const string Process = Default + ".Process";
        public const string GlobalView = Default + ".GlobalView";
    }

    public static class Profiles
    {
        public const string Default = GroupName + ".Profiles";
        public const string Manage = Default + ".Manage";
        public const string ViewAll = Default + ".ViewAll";
        public const string Search = Default + ".Search";
        public const string Edit = Default + ".Edit";
    }

    public static class Directory
    {
        public const string Default = GroupName + ".Directory";
        public const string Search = Default + ".Search";
        public const string ManageCache = Default + ".ManageCache";
    }

    public static class Reporting
    {
        public const string Default = GroupName + ".Reporting";
        public const string BasicReport = Default + ".BasicReport";
        public const string GraduatesReport = Default + ".GraduatesReport";
    }

    public static class Communication
    {
        public const string Default = GroupName + ".Communication";
        public const string SendMassMessage = Default + ".SendMassMessage";
    }

    public static class Gallery
    {
        public const string Default = GroupName + ".Gallery";
        public const string Upload = Default + ".Upload";
        public const string Delete = Default + ".Delete";
    }

    public static class Health
    {
        public const string Default = GroupName + ".Health";
        public const string Manage = Default + ".Manage";
        public const string ViewOffers = Default + ".ViewOffers";
    }

    public static class Magazine
    {
        public const string Default = GroupName + ".Magazine";
        public const string ManageIssues = Default + ".ManageIssues";
        public const string ManagePosts = Default + ".ManagePosts";
        public const string ApproveComments = Default + ".ApproveComments";
    }

    public static class Guidance
    {
        public const string Default = GroupName + ".Guidance";
        public const string ManageAvailability = Default + ".ManageAvailability";
        public const string BookSession = Default + ".BookSession";
        public const string ManageRequests = Default + ".ManageRequests";
        public const string ViewAllBranches = Default + ".ViewAllBranches";
    }

    public static class Benefits
    {
        public const string Default = GroupName + ".Benefits";
        public const string Manage = Default + ".Manage";
        public const string View = Default + ".View";
    }

    public static class Career
    {
        public const string Default = GroupName + ".Career";
        public const string Manage = Default + ".Manage";
        public const string Register = Default + ".Register";
    }

    public static class Syndicates
    {
        public const string Default = GroupName + ".Syndicates";
        public const string Manage = Default + ".Manage";
        public const string Apply = Default + ".Apply";
    }

    public static class Events
    {
        public const string Default = GroupName + ".Events";
        public const string Manage = Default + ".Manage";
        public const string Register = Default + ".Register";
        public const string VerifyTicket = Default + ".VerifyTicket";
    }

    public static class Companies
    {
        public const string Default = GroupName + ".Companies";
        public const string Manage = Default + ".Manage";
    }

    public static class Careers
    {
        public const string Default = GroupName + ".Careers";
        public const string JobManage = Default + ".JobManage";
        public const string JobApply = Default + ".JobApply";
        public const string CvAudit = Default + ".CvAudit";
        public const string CvManage = Default + ".CvManage";
    }

    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
        public const string ViewStatistics = Default + ".ViewStatistics";
        public const string ViewFinancials = Default + ".ViewFinancials"; // President Only
    }

    public static class Trips
    {
        public const string Default = GroupName + ".Trips";
        public const string Manage = Default + ".Manage";
        public const string Request = Default + ".Request";
    }

    public static class Healthcare
    {
        public const string Default = GroupName + ".Healthcare";
        public const string View = Default + ".View";
        public const string Manage = Default + ".Manage";
    }

    public static class Admin
    {
        public const string Default = GroupName + ".Admin";
        public const string AlumniManage = Default + ".AlumniManage";
        public const string AlumniApprove = Default + ".AlumniApprove";
        public const string JobModerate = Default + ".JobModerate";
        public const string EventManage = Default + ".EventManage";
        public const string Dashboard = Default + ".Dashboard";
        public const string ContentModerate = Default + ".ContentModerate";
        public const string GuidanceManage = Default + ".GuidanceManage";

        // Phase 2
        public const string CertificateManage = Default + ".CertificateManage";
        public const string MembershipManage = Default + ".MembershipManage";
        public const string SyndicateManage = Default + ".SyndicateManage";
        public const string TripManage = Default + ".TripManage";
        public const string GalleryManage = Default + ".GalleryManage";
        public const string HealthcareManage = Default + ".HealthcareManage";
        public const string BenefitsManage = Default + ".BenefitsManage";
    }
}
