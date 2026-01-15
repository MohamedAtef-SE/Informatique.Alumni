namespace Informatique.Alumni.Health;

/// <summary>
/// Constants for the Health module.
/// Clean Code: Centralized constant management.
/// </summary>
public static class HealthConsts
{
    /// <summary>
    /// Maximum length for healthcare offer titles.
    /// </summary>
    public const int MaxOfferTitleLength = 256;
    
    /// <summary>
    /// Allowed file extensions for healthcare offer files.
    /// Business Rule: PDF and images only.
    /// </summary>
    public static readonly string[] AllowedOfferFileExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
}
