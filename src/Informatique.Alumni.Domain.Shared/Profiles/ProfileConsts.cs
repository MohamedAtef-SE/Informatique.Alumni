namespace Informatique.Alumni.Profiles;

public static class ProfileConsts
{
    public const string MobileNumberRegex = @"^01[0125][0-9]{8}$"; // Egyptian Mobile Numbers
    public const string NationalIdRegex = @"^[23][0-9]{13}$"; // Egyptian National ID
    
    public const int MaxJobTitleLength = 128;
    public const int MaxBioLength = 2000;
    public const int MaxPlaceLength = 256;
    public const int MaxMobileLength = 32;
    public const int MaxNationalIdLength = 32;
}
