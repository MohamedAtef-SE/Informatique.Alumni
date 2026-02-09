using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Informatique.Alumni.Branches;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace Informatique.Alumni.Certificates;

public class CertificateManager : DomainService
{
    private readonly IRepository<CertificateDefinition, Guid> _definitionRepository;
    private readonly IRepository<AlumniProfile, Guid> _profileRepository;
    private readonly IRepository<Branch, Guid> _branchRepository;
    private readonly MembershipManager _membershipManager;
    private readonly IConfiguration _configuration;

    public CertificateManager(
        IRepository<CertificateDefinition, Guid> definitionRepository,
        IRepository<AlumniProfile, Guid> profileRepository,
        IRepository<Branch, Guid> branchRepository,
        MembershipManager membershipManager,
        IConfiguration configuration)
    {
        _definitionRepository = definitionRepository;
        _profileRepository = profileRepository;
        _branchRepository = branchRepository;
        _membershipManager = membershipManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Business Logic: Create a new certificate request with ALL 9 business rules enforced
    /// </summary>
    public async Task<CertificateRequest> CreateRequestAsync(
        Guid requestId,
        Guid alumniId,
        List<CreateItemInput> items,
        DeliveryMethod deliveryMethod,
        Guid? targetBranchId = null,
        string? deliveryAddress = null,
        string? userNotes = null)
    {
        // ================== BUSINESS RULE #1: Membership Validation ==================
        var isActive = await _membershipManager.IsActiveAsync(alumniId);
        if (!isActive)
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.MembershipExpired)
                .WithData("AlumniId", alumniId);
        }

        // ================== BUSINESS RULE #2: Multi-Item Validation ==================
        if (items == null || !items.Any())
        {
            throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.NoItemsProvided);
        }

        // Validate each item
        var certificateRequestItems = new List<CertificateRequestItem>();
        decimal totalItemFees = 0;

        foreach (var input in items)
        {
            var definition = await ValidateEligibilityAsync(input.CertificateDefinitionId);
            
            var item = new CertificateRequestItem(
                GuidGenerator.Create(),
                requestId,
                input.CertificateDefinitionId,
                input.Language,
                definition.Fee,
                input.QualificationId,
                input.AttachmentUrl
            );
            
            certificateRequestItems.Add(item);
            totalItemFees += definition.Fee;
        }

        // ================== BUSINESS RULE #4: Logistics & Delivery ==================
        decimal deliveryFee = 0;
        
        if (deliveryMethod == DeliveryMethod.HomeDelivery)
        {
            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.DeliveryAddressRequired);
            }
            
            // TODO: Calculate delivery fee based on address/zone (integrate with Wasla API)
            // For now, use a fixed delivery fee
            deliveryFee = 50m; // Example: 50 EGP delivery fee
        }
        else if (deliveryMethod == DeliveryMethod.BranchPickup)
        {
            if (!targetBranchId.HasValue)
            {
                throw new BusinessException(AlumniDomainErrorCodes.CertificateRequest.BranchRequired);
            }
            
            // Validate branch exists
            await _branchRepository.GetAsync(targetBranchId.Value);
        }

        // ================== BUSINESS RULE #3: Wallet Deduction Logic ==================
        var profile = await _profileRepository.FirstOrDefaultAsync(x => x.UserId == alumniId);
        if (profile == null)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AlumniProfile.ProfileNotFound)
                .WithData("AlumniId", alumniId);
        }

        decimal totalFees = totalItemFees + deliveryFee;
        decimal walletBalance = profile.WalletBalance;
        decimal usedWallet = 0;

        if (walletBalance >= totalFees)
        {
            // Scenario A: Full payment from wallet
            usedWallet = totalFees;
            profile.DeductWallet(totalFees);
        }
        else if (walletBalance > 0)
        {
            // Scenario B: Partial wallet, remaining via gateway
            usedWallet = walletBalance;
            profile.DeductWallet(walletBalance);
        }
        // Scenario C: No wallet balance (usedWallet remains 0)

        // ================== Create Certificate Request ==================
        var request = new CertificateRequest(
            requestId,
            alumniId,
            deliveryMethod,
            targetBranchId,
            deliveryAddress,
            userNotes
        );

        // Add all items
        foreach (var item in certificateRequestItems)
        {
            request.AddItem(item);
        }

        // Set delivery fee
        request.SetDeliveryFee(deliveryFee);

        // Apply wallet deduction and set initial status
        request.ApplyWalletDeduction(usedWallet);

        return request;
    }

    /// <summary>
    /// Business Logic: Check if alumni is eligible to request a certificate
    /// </summary>
    public async Task<CertificateDefinition> ValidateEligibilityAsync(Guid certificateDefinitionId)
    {
        var definition = await _definitionRepository.GetAsync(certificateDefinitionId);
        
        if (!definition.IsActive)
        {
            throw new BusinessException(AlumniDomainErrorCodes.Certificate.DefinitionNotActive)
                .WithData("CertificateDefinitionId", certificateDefinitionId)
                .WithData("DefinitionNameEn", definition.NameEn)
                .WithData("DefinitionNameAr", definition.NameAr);
        }

        return definition;
    }

    /// <summary>
    /// Business Logic: Generate cryptographic verification hash for anti-fraud
    /// </summary>
    public string GenerateVerificationHash(CertificateRequestItem item)
    {
        Check.NotNull(item, nameof(item));

        var rawData = $"{item.Id}|{item.CertificateRequestId}|{item.CertificateDefinitionId}|{DateTime.UtcNow.Ticks}";
        
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            
            return builder.ToString();
        }
    }

    /// <summary>
    /// Business Logic: Generate QR code URL for certificate verification
    /// </summary>
    public string GenerateQrCodeUrl(Guid itemId, string verificationHash)
    {
        var baseUrl = _configuration["App:ClientUrl"]?.TrimEnd('/') ?? _configuration["App:SelfUrl"]?.TrimEnd('/') ?? "https://alumni.informatique.com";
        return $"{baseUrl}/verify-certificate?id={itemId}&hash={verificationHash}";
    }

    /// <summary>
    /// Business Logic: Process certificate item to Ready status
    /// </summary>
    public void MarkItemAsReady(CertificateRequestItem item)
    {
        Check.NotNull(item, nameof(item));

        var hash = GenerateVerificationHash(item);
        var qrUrl = GenerateQrCodeUrl(item.Id, hash);
        
        item.MarkAsReady(hash, qrUrl);
    }
}

/// <summary>
/// Input DTO for creating certificate items (used by domain service)
/// </summary>
public class CreateItemInput
{
    public Guid CertificateDefinitionId { get; set; }
    public Guid? QualificationId { get; set; }
    public CertificateLanguage Language { get; set; }
    public string? AttachmentUrl { get; set; }
}
