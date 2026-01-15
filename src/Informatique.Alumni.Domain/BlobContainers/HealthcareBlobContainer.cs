using Volo.Abp.BlobStoring;

namespace Informatique.Alumni.BlobContainers;

/// <summary>
/// Blob container for healthcare offer files.
/// Stores: PDF files and images (JPG, JPEG, PNG) uploaded by employees.
/// Clean Code: Attribute-based configuration.
/// </summary>
[BlobContainerName("healthcare-offers")]
public class HealthcareBlobContainer
{
}
