using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Informatique.Alumni.Certificates;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using Informatique.Alumni.Branches;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

using CertDeliveryMethod = Informatique.Alumni.Certificates.DeliveryMethod;

namespace Informatique.Alumni.Domain.Tests.Certificates;

public class CertificateManagerTests
{
    private readonly IRepository<CertificateDefinition, Guid> _mockDefinitionRepository;
    private readonly IRepository<AlumniProfile, Guid> _mockProfileRepository;
    private readonly IRepository<Branch, Guid> _mockBranchRepository;
    private readonly MembershipManager _membershipManager;
    private readonly CertificateManager _certificateManager;

    public CertificateManagerTests()
    {
        _mockDefinitionRepository = Substitute.For<IRepository<CertificateDefinition, Guid>>();
        _mockProfileRepository = Substitute.For<IRepository<AlumniProfile, Guid>>();
        _mockBranchRepository = Substitute.For<IRepository<Branch, Guid>>();

        // Setup MembershipManager dependencies
        var feeRepo = Substitute.For<IRepository<SubscriptionFee, Guid>>();
        var reqRepo = Substitute.For<IRepository<AssociationRequest, Guid>>();
        var payRepo = Substitute.For<IRepository<PaymentTransaction, Guid>>();
        var profileRepo = Substitute.For<IRepository<AlumniProfile, Guid>>();
        var configRepo = Substitute.For<IRepository<MembershipFeeConfig, Guid>>();
        
        // Mock MembershipManager partially or fully
        _membershipManager = Substitute.ForPartsOf<MembershipManager>(feeRepo, reqRepo, payRepo, profileRepo, configRepo);

        var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();

        _certificateManager = new CertificateManager(
            _mockDefinitionRepository,
            _mockProfileRepository,
            _mockBranchRepository,
            _membershipManager,
            configuration
        );
    }
    
    private AlumniProfile CreateProfile(Guid id, Guid userId)
    {
        return new AlumniProfile(id, userId, "01234567890", "12345678901234");
    }

    #region ValidateEligibilityAsync Tests

    [Fact]
    public async Task ValidateEligibilityAsync_Should_Return_Definition_When_Active()
    {
        // Arrange
        var definitionId = Guid.NewGuid();
        var activeDef = new CertificateDefinition(definitionId, "شهادة تخرج", "Graduation Certificate", 100m, DegreeType.Undergraduate);
        
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(activeDef);

        // Act
        var result = await _certificateManager.ValidateEligibilityAsync(definitionId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(definitionId);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateEligibilityAsync_Should_Throw_When_Definition_NotActive()
    {
        // Arrange
        var definitionId = Guid.NewGuid();
        var inactiveDef = new CertificateDefinition(definitionId, "شهادة قديمة", "Old Certificate", 50m, DegreeType.Postgraduate);
        inactiveDef.Deactivate(); 
        
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(inactiveDef);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _certificateManager.ValidateEligibilityAsync(definitionId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Certificate.DefinitionNotActive);
        exception.Data["CertificateDefinitionId"].ShouldBe(definitionId);
    }

    #endregion

    #region CreateRequestAsync Tests

    [Fact]
    public async Task CreateRequestAsync_Should_Create_Request_When_Valid()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        var userNotes = "Please expedite";
        var branchId = Guid.NewGuid();
        
        var activeDef = new CertificateDefinition(definitionId, "شهادة تخرج", "Graduation Certificate", 100m, DegreeType.Undergraduate);
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(activeDef);

        var profile = CreateProfile(alumniId, Guid.NewGuid());
        profile.AddCredit(1000m);
        _mockProfileRepository.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<AlumniProfile, bool>>>())
            .Returns(profile);
            
        _mockBranchRepository.GetAsync(branchId).Returns(new Branch(branchId, "Main", "Main"));
        _membershipManager.IsActiveAsync(alumniId).Returns(Task.FromResult(true));

        var items = new List<CreateItemInput>
        {
            new CreateItemInput { CertificateDefinitionId = definitionId, Language = CertificateLanguage.English }
        };

        // Act
        var request = await _certificateManager.CreateRequestAsync(
            requestId,
            alumniId,
            items,
            CertDeliveryMethod.BranchPickup,
            branchId,
            null,
            userNotes
        );

        // Assert
        request.ShouldNotBeNull();
        request.Id.ShouldBe(requestId);
        request.AlumniId.ShouldBe(alumniId);
        request.UserNotes.ShouldBe(userNotes);
    }

    [Fact]
    public async Task CreateRequestAsync_Should_Throw_When_Definition_NotActive()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        
        var inactiveDef = new CertificateDefinition(definitionId, "شهادة مهملة", "Deprecated Certificate", 50m, DegreeType.Postgraduate);
        inactiveDef.Deactivate(); 
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(inactiveDef);
            
        _membershipManager.IsActiveAsync(alumniId).Returns(Task.FromResult(true));

        var items = new List<CreateItemInput>
        {
            new CreateItemInput { CertificateDefinitionId = definitionId, Language = CertificateLanguage.English }
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _certificateManager.CreateRequestAsync(requestId, alumniId, items, CertDeliveryMethod.BranchPickup, branchId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Certificate.DefinitionNotActive);
    }

    #endregion

    #region GenerateVerificationHash Tests

    [Fact]
    public void GenerateVerificationHash_Should_Return_NonEmpty_Hash()
    {
        // Arrange
        var item = new CertificateRequestItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            CertificateLanguage.English,
             100m,
             null
        );

        // Act
        var hash = _certificateManager.GenerateVerificationHash(item);

        // Assert
        hash.ShouldNotBeNullOrEmpty();
        hash.Length.ShouldBe(64); 
    }

    #endregion

    #region GenerateQrCodeUrl Tests

    [Fact]
    public void GenerateQrCodeUrl_Should_Return_Valid_Url()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var hash = "abc123def456";

        // Act
        var url = _certificateManager.GenerateQrCodeUrl(itemId, hash);

        // Assert
        url.ShouldNotBeNullOrEmpty();
        url.ShouldContain(itemId.ToString());
        url.ShouldContain(hash);
        url.ShouldStartWith("https://");
    }

    #endregion
}
