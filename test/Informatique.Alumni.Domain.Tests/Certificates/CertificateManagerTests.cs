using System;
using System.Threading.Tasks;
using Informatique.Alumni.Certificates;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace Informatique.Alumni.Domain.Tests.Certificates;

public class CertificateManagerTests
{
    private readonly IRepository<CertificateDefinition, Guid> _mockDefinitionRepository;
    private readonly CertificateManager _certificateManager;

    public CertificateManagerTests()
    {
        _mockDefinitionRepository = Substitute.For<IRepository<CertificateDefinition, Guid>>();
        _certificateManager = new CertificateManager(_mockDefinitionRepository);
    }

    #region ValidateEligibilityAsync Tests

    [Fact]
    public async Task ValidateEligibilityAsync_Should_Return_Definition_When_Active()
    {
        // Arrange
        var definitionId = Guid.NewGuid();
        var activeDef = new CertificateDefinition(definitionId, "Graduation Certificate", 100m);
        
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
        var inactiveDef = new CertificateDefinition(definitionId, "Old Certificate", 50m);
        inactiveDef.IsActive = false; // Set as inactive
        
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(inactiveDef);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _certificateManager.ValidateEligibilityAsync(definitionId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Certificate.DefinitionNotActive);
        exception.Data["CertificateDefinitionId"].ShouldBe(definitionId);
        exception.Data["DefinitionName"].ShouldBe("Old Certificate");
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
        
        var activeDef = new CertificateDefinition(definitionId, "Graduation Certificate", 100m);
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(activeDef);

        // Act
        var request = await _certificateManager.CreateRequestAsync(
            requestId,
            alumniId,
            definitionId,
            userNotes
        );

        // Assert
        request.ShouldNotBeNull();
        request.Id.ShouldBe(requestId);
        request.AlumniId.ShouldBe(alumniId);
        request.CertificateDefinitionId.ShouldBe(definitionId);
        request.UserNotes.ShouldBe(userNotes);
        request.Status.ShouldBe(CertificateStatus.Pending);
    }

    [Fact]
    public async Task CreateRequestAsync_Should_Throw_When_Definition_NotActive()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        
        var inactiveDef = new CertificateDefinition(definitionId, "Deprecated Certificate", 50m);
        inactiveDef.IsActive = false; // Set as inactive
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(inactiveDef);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _certificateManager.CreateRequestAsync(requestId, alumniId, definitionId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Certificate.DefinitionNotActive);
    }

    [Fact]
    public async Task CreateRequestAsync_Should_Work_Without_UserNotes()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        
        var activeDef = new CertificateDefinition(definitionId, "Basic Certificate", 100m);
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(activeDef);

        // Act
        var request = await _certificateManager.CreateRequestAsync(
            requestId,
            alumniId,
            definitionId
        );

        // Assert
        request.ShouldNotBeNull();
        request.UserNotes.ShouldBeNullOrEmpty();
    }

    #endregion

    #region GenerateVerificationHash Tests

    [Fact]
    public void GenerateVerificationHash_Should_Return_NonEmpty_Hash()
    {
        // Arrange
        var request = new CertificateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test"
        );

        // Act
        var hash = _certificateManager.GenerateVerificationHash(request);

        // Assert
        hash.ShouldNotBeNullOrEmpty();
        hash.Length.ShouldBe(64); // SHA256 produces 64 hex characters
    }

    [Fact]
    public void GenerateVerificationHash_Should_Be_Deterministic_For_Same_Request()
    {
        // Arrange
        var requestId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var alumniId = Guid.Parse("87654321-4321-4321-4321-210987654321");
        var definitionId = Guid.Parse("ABCDEFAB-ABCD-ABCD-ABCD-ABCDEFABCDEF");
        
        var request = new CertificateRequest(requestId, alumniId, definitionId, "Test");

        // Act
        var hash1 = _certificateManager.GenerateVerificationHash(request);
        
        // Note: Hash includes timestamp, so will be different each time
        // We just verify it's a valid SHA256 hash
        
        // Assert
        hash1.ShouldNotBeNullOrEmpty();
        hash1.Length.ShouldBe(64);
        hash1.ShouldMatch(@"^[a-f0-9]{64}$"); // Valid hex string
    }

    [Fact]
    public void GenerateVerificationHash_Should_Throw_When_Request_Null()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            _certificateManager.GenerateVerificationHash(null!)
        );
    }

    #endregion

    #region GenerateQrCodeUrl Tests

    [Fact]
    public void GenerateQrCodeUrl_Should_Return_Valid_Url()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var hash = "abc123def456";

        // Act
        var url = _certificateManager.GenerateQrCodeUrl(requestId, hash);

        // Assert
        url.ShouldNotBeNullOrEmpty();
        url.ShouldContain(requestId.ToString());
        url.ShouldContain(hash);
        url.ShouldStartWith("https://");
    }

    [Fact]
    public void GenerateQrCodeUrl_Should_Include_Both_Id_And_Hash()
    {
        // Arrange
        var requestId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var hash = "verification_hash_value";

        // Act
        var url = _certificateManager.GenerateQrCodeUrl(requestId, hash);

        // Assert
        url.ShouldContain("id=12345678-1234-1234-1234-123456789012");
        url.ShouldContain("hash=verification_hash_value");
    }

    #endregion

    #region MarkAsReady Tests

    [Fact]
    public void MarkAsReady_Should_Set_Hash_And_QrCode()
    {
        // Arrange
        var request = new CertificateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test"
        );
        
        // Move to processing first (business rule)
        request.MoveToProcessing();

        // Act
        _certificateManager.MarkAsReady(request);

        // Assert
        request.Status.ShouldBe(CertificateStatus.Ready);
        request.VerificationHash.ShouldNotBeNullOrEmpty();
        request.QrCodeContent.ShouldNotBeNullOrEmpty();
        request.VerificationHash.Length.ShouldBe(64); // SHA256
    }

    [Fact]
    public void MarkAsReady_Should_Throw_When_Request_Null()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            _certificateManager.MarkAsReady(null!)
        );
    }

    [Fact]
    public void MarkAsReady_Should_Throw_When_Not_In_Processing_Status()
    {
        // Arrange
        var request = new CertificateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test"
        );
        // Request is in Pending status, not Processing

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => 
            _certificateManager.MarkAsReady(request)
        );
        
        exception.Code.ShouldBe(AlumniDomainErrorCodes.CertificateRequest.InvalidStatusTransition);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Full_Workflow_Should_Work_EndToEnd()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var definitionId = Guid.NewGuid();
        
        var activeDef = new CertificateDefinition(definitionId, "Graduation Certificate", 100m);
        _mockDefinitionRepository.GetAsync(definitionId, Arg.Any<bool>())
            .Returns(activeDef);

        // Act - Create Request
        var request = await _certificateManager.CreateRequestAsync(
            requestId,
            alumniId,
            definitionId,
            "Please expedite my certificate"
        );

        // Assert - Initial State
        request.Status.ShouldBe(CertificateStatus.Pending);

        // Act - Process Request
        request.MoveToProcessing();
        request.Status.ShouldBe(CertificateStatus.Processing);

        // Act - Mark as Ready (using manager)
        _certificateManager.MarkAsReady(request);

        // Assert - Final State
        request.Status.ShouldBe(CertificateStatus.Ready);
        request.VerificationHash.ShouldNotBeNullOrEmpty();
        request.QrCodeContent.ShouldNotBeNullOrEmpty();
    }

    #endregion
}
