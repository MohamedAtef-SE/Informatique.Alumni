namespace Informatique.Alumni;

public static class AlumniDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */
    
    public static class Job
    {
        public const string InvalidClosingDate = "Alumni:Job:001";
        public const string AlreadyClosed = "Alumni:Job:002";
        public const string AlreadyActive = "Alumni:Job:003";
        public const string CannotReopenExpired = "Alumni:Job:004";
    }

    public static class AssociationEvent
    {
        public const string InvalidCapacity = "Alumni:Event:001";
        public const string AlreadyPublished = "Alumni:Event:002";
        public const string NotPublished = "Alumni:Event:003";
        public const string AgendaItemOutsideEventDate = "Alumni:Event:004";
        public const string AgendaItemNotFound = "Alumni:Event:005";
    }

    public static class EventAgendaItem
    {
        public const string InvalidTimeRange = "Alumni:AgendaItem:001";
    }

    public static class Trip
    {
        public const string InvalidDateRange = "Alumni:Trip:001";
        public const string InvalidCapacity = "Alumni:Trip:002";
        public const string InvalidPrice = "Alumni:Trip:003";
        public const string AlreadyActive = "Alumni:Trip:004";
        public const string AlreadyInactive = "Alumni:Trip:005";
    }

    public static class CertificateRequest
    {
        public const string InvalidStatusTransition = "Alumni:Certificate:001";
        public const string CannotRejectDelivered = "Alumni:Certificate:002";
        public const string MembershipExpired = "Alumni:Certificate:003";
        public const string NoItemsProvided = "Alumni:Certificate:004";
        public const string DeliveryAddressRequired = "Alumni:Certificate:005";
        public const string BranchRequired = "Alumni:Certificate:006";
        public const string CannotModifyNonDraft = "Alumni:Certificate:007";
        public const string ItemNotFound = "Alumni:Certificate:008";
        public const string InvalidDeliveryFee = "Alumni:Certificate:009";
        public const string NegativeWalletDeduction = "Alumni:Certificate:010";
        public const string ExcessiveWalletDeduction = "Alumni:Certificate:011";
        public const string InvalidPaymentAmount = "Alumni:Certificate:012";
        public const string InvalidStatusForPayment = "Alumni:Certificate:013";
        public const string PaymentRequired = "Alumni:Certificate:014";
        public const string InvalidDeliveryMethod = "Alumni:Certificate:015";
        public const string Unauthorized = "Alumni:Certificate:016";
        public const string InvalidStatusForDeliveryMethod = "Alumni:Certificate:017";
        public const string CannotModifyDelivered = "Alumni:Certificate:018";
        public const string CannotModifyRejected = "Alumni:Certificate:019";
    }

    public static class AlumniProfile
    {
        public const string InvalidMobileFormat = "Alumni:Profile:001";
        public const string DuplicateExperience = "Alumni:Profile:002";
        public const string ExperienceNotFound = "Alumni:Profile:003";
        public const string DuplicateEducation = "Alumni:Profile:004";
        public const string EducationNotFound = "Alumni:Profile:005";
        public const string NegativeDeduction = "Alumni:Profile:006";
        public const string InsufficientBalance = "Alumni:Profile:007";
        public const string InvalidCreditAmount = "Alumni:Profile:008";
        public const string ProfileNotFound = "Alumni:Profile:009";
    }

    public static class SubscriptionFee
    {
        public const string InvalidAmount = "Alumni:Subscription:001";
        public const string InvalidDateRange = "Alumni:Subscription:002";
        public const string InvalidYear = "Alumni:Subscription:003";
        public const string AlreadyActive = "Alumni:Subscription:004";
        public const string AlreadyInactive = "Alumni:Subscription:005";
        public const string CannotCloseBeforeEnd = "Alumni:Subscription:006";
    }

    public static class Certificate
    {
        public const string DefinitionNotActive = "Alumni:Certificate:001";
        public const string InvalidFee = "Alumni:Certificate:002";
        public const string DuplicateNameAr = "Alumni:Certificate:003";
        public const string DuplicateNameEn = "Alumni:Certificate:004";
        public const string CannotDeleteUsedDefinition = "Alumni:Certificate:005";
    }

    public static class Membership
    {
        public const string SubscriptionClosed = "Alumni:Membership:001";
        public const string NoValidPayment = "Alumni:Membership:002";
        public const string UnauthorizedPayment = "Alumni:Membership:003";
        public const string RequestNotPayable = "Alumni:Membership:004";
        public const string InactiveMembership = "Alumni:Membership:005";
    }
}

