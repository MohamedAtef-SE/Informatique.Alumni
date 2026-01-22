using System;
using System.Threading.Tasks;
using Informatique.Alumni.Membership;
using Informatique.Alumni.Profiles;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

using MemDeliveryMethod = Informatique.Alumni.Membership.DeliveryMethod;

namespace Informatique.Alumni.Domain.Tests.Membership;

public class MembershipManagerTests
{
    private readonly IRepository<SubscriptionFee, Guid> _mockFeeRepository;
    private readonly IRepository<AssociationRequest, Guid> _mockRequestRepository;
    private readonly IRepository<PaymentTransaction, Guid> _mockPaymentRepository;
    private readonly IRepository<AlumniProfile, Guid> _mockProfileRepository;
    private readonly IRepository<MembershipFeeConfig, Guid> _mockConfigRepository;
    private readonly MembershipManager _membershipManager;

    public MembershipManagerTests()
    {
        _mockFeeRepository = Substitute.For<IRepository<SubscriptionFee, Guid>>();
        _mockRequestRepository = Substitute.For<IRepository<AssociationRequest, Guid>>();
        _mockPaymentRepository = Substitute.For<IRepository<PaymentTransaction, Guid>>();
        _mockProfileRepository = Substitute.For<IRepository<AlumniProfile, Guid>>();
        _mockConfigRepository = Substitute.For<IRepository<MembershipFeeConfig, Guid>>();
        
        _membershipManager = new MembershipManager(
            _mockFeeRepository,
            _mockRequestRepository,
            _mockPaymentRepository,
            _mockProfileRepository,
            _mockConfigRepository
        );
        
        var mockClock = Substitute.For<Volo.Abp.Timing.IClock>();
        mockClock.Now.Returns(DateTime.UtcNow);
        
        var mockGuidGenerator = Substitute.For<Volo.Abp.Guids.IGuidGenerator>();
        mockGuidGenerator.Create().Returns(Guid.NewGuid());
        
        var mockLazyServiceProvider = Substitute.For<Volo.Abp.DependencyInjection.IAbpLazyServiceProvider>();
        mockLazyServiceProvider.LazyGetRequiredService<Volo.Abp.Timing.IClock>()
            .ReturnsForAnyArgs(mockClock);
        mockLazyServiceProvider.LazyGetRequiredService<Volo.Abp.Guids.IGuidGenerator>()
            .ReturnsForAnyArgs(mockGuidGenerator);
            
        _membershipManager.LazyServiceProvider = mockLazyServiceProvider;
    }

    #region ValidateSubscriptionFeeAsync Tests

    [Fact]
    public async Task ValidateSubscriptionFeeAsync_Should_Return_Fee_When_Valid()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        var validFee = new SubscriptionFee(
            feeId,
            "2024 Membership",
            100m,
            2024,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20)
        );
        
        _mockFeeRepository.GetAsync(feeId, Arg.Any<bool>())
            .Returns(validFee);

        // Act
        var result = await _membershipManager.ValidateSubscriptionFeeAsync(feeId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(feeId);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task ValidateSubscriptionFeeAsync_Should_Throw_When_Fee_Inactive()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        var inactiveFee = new SubscriptionFee(
            feeId,
            "2023 Membership (Closed)",
            100m,
            2023,
            DateTime.UtcNow.AddDays(-365),
            DateTime.UtcNow.AddDays(-1) // Ended yesterday
        );
        inactiveFee.Deactivate();
        
        _mockFeeRepository.GetAsync(feeId, Arg.Any<bool>())
            .Returns(inactiveFee);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _membershipManager.ValidateSubscriptionFeeAsync(feeId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Membership.SubscriptionClosed);
        exception.Data["SubscriptionFeeId"].ShouldBe(feeId);
    }

    [Fact]
    public async Task ValidateSubscriptionFeeAsync_Should_Throw_When_Season_Ended()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        var expiredFee = new SubscriptionFee(
            feeId,
            "Expired Membership",
            100m,
            2023,
            DateTime.UtcNow.AddDays(-100),
            DateTime.UtcNow.AddDays(-10) // Ended 10 days ago
        );
        
        _mockFeeRepository.GetAsync(feeId, Arg.Any<bool>())
            .Returns(expiredFee);

        // Act & Assert
        await Should.ThrowAsync<BusinessException>(
            async () => await _membershipManager.ValidateSubscriptionFeeAsync(feeId)
        );
    }

    #endregion

    #region CheckIdempotencyAsync Tests

    [Fact]
    public async Task CheckIdempotencyAsync_Should_Return_Null_When_No_Duplicate()
    {
        // Arrange
        var idempotencyKey = "unique-key-12345";
        
        _mockRequestRepository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<AssociationRequest, bool>>>())
            .Returns(new System.Collections.Generic.List<AssociationRequest>());

        // Act
        var result = await _membershipManager.CheckIdempotencyAsync(idempotencyKey);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CheckIdempotencyAsync_Should_Return_Existing_When_Duplicate()
    {
        // Arrange
        var idempotencyKey = "duplicate-key-67890";
        var existingRequest = new AssociationRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            idempotencyKey,
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );
        
        _mockRequestRepository.GetListAsync(
                Arg.Any<System.Linq.Expressions.Expression<System.Func<AssociationRequest, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<System.Threading.CancellationToken>()
            )
            .ReturnsForAnyArgs(new System.Collections.Generic.List<AssociationRequest> { existingRequest });

        // Act
        var result = await _membershipManager.CheckIdempotencyAsync(idempotencyKey);

        // Assert
        result.ShouldNotBeNull();
        result.IdempotencyKey.ShouldBe(idempotencyKey);
    }

    [Fact]
    public async Task CheckIdempotencyAsync_Should_Throw_When_Key_NullOrEmpty()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(
            async () => await _membershipManager.CheckIdempotencyAsync(string.Empty)
        );
    }

    #endregion

    #region CreateMembershipRequestAsync Tests

    [Fact]
    public async Task CreateMembershipRequestAsync_Should_Create_New_Request_When_No_Duplicate()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var idempotencyKey = "unique-request-key";
        var branchId = Guid.NewGuid();
        
        var validFee = new SubscriptionFee(
            feeId,
            "2024 Membership",
            100m,
            2024,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20)
        );
        
        _mockFeeRepository.GetAsync(feeId, Arg.Any<bool>())
            .Returns(validFee);
        
        _mockRequestRepository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<AssociationRequest, bool>>>())
            .Returns(new System.Collections.Generic.List<AssociationRequest>());

        // Act
        var request = await _membershipManager.CreateMembershipRequestAsync(
            requestId,
            alumniId,
            feeId,
            idempotencyKey,
            branchId,
            2024,
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );

        // Assert
        request.ShouldNotBeNull();
        request.Id.ShouldBe(requestId);
        request.AlumniId.ShouldBe(alumniId);
        request.SubscriptionFeeId.ShouldBe(feeId);
        request.IdempotencyKey.ShouldBe(idempotencyKey);
        request.Status.ShouldBe(MembershipRequestStatus.Pending);
    }

    [Fact]
    public async Task CreateMembershipRequestAsync_Should_Return_Existing_When_Duplicate_Key()
    {
        // Arrange
        var existingRequestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var idempotencyKey = "duplicate-key-12345";
        
        var existingRequest = new AssociationRequest(
            existingRequestId,
            alumniId,
            feeId,
            idempotencyKey,
            branchId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );
        
        _mockRequestRepository.GetListAsync(
                Arg.Any<System.Linq.Expressions.Expression<System.Func<AssociationRequest, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<System.Threading.CancellationToken>()
            )
            .ReturnsForAnyArgs(new System.Collections.Generic.List<AssociationRequest> { existingRequest });

        // Act
        var request = await _membershipManager.CreateMembershipRequestAsync(
            Guid.NewGuid(), // Different ID - should be ignored
            alumniId,
            feeId,
            idempotencyKey,
            branchId,
            2024,
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );

        // Assert
        request.ShouldNotBeNull();
        request.Id.ShouldBe(existingRequestId); // Returns existing, not new ID
        request.IdempotencyKey.ShouldBe(idempotencyKey);
    }

    [Fact]
    public async Task CreateMembershipRequestAsync_Should_Throw_When_Fee_Invalid()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var idempotencyKey = "test-key";
        
        var invalidFee = new SubscriptionFee(
            feeId,
            "Closed Membership",
            100m,
            2023,
            DateTime.UtcNow.AddDays(-100),
            DateTime.UtcNow.AddDays(-1) // Ended
        );
        
        _mockFeeRepository.GetAsync(feeId, Arg.Any<bool>())
            .Returns(invalidFee);
        
        _mockRequestRepository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<AssociationRequest, bool>>>())
            .Returns(new System.Collections.Generic.List<AssociationRequest>());

        // Act & Assert
        await Should.ThrowAsync<BusinessException>(
            async () => await _membershipManager.CreateMembershipRequestAsync(
                requestId, alumniId, feeId, idempotencyKey, branchId, 2023, MemDeliveryMethod.OfficePickup, 0, 0, 0, null
            )
        );
    }

    #endregion

    #region ValidatePaymentForApprovalAsync Tests - CRITICAL

    [Fact]
    public async Task ValidatePaymentForApprovalAsync_Should_Pass_When_Payment_Exists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        
        _mockPaymentRepository.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PaymentTransaction, bool>>>())
            .Returns(true);

        // Act & Assert
        await _membershipManager.ValidatePaymentForApprovalAsync(requestId); // Should not throw
    }

    [Fact]
    public async Task ValidatePaymentForApprovalAsync_Should_Throw_When_No_Payment()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        
        _mockPaymentRepository.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PaymentTransaction, bool>>>())
            .Returns(false);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _membershipManager.ValidatePaymentForApprovalAsync(requestId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Membership.NoValidPayment);
        exception.Data["RequestId"].ShouldBe(requestId);
    }

    #endregion

    #region ApproveRequestAsync Tests

    [Fact]
    public async Task ApproveRequestAsync_Should_Approve_When_Payment_Valid()
    {
        // Arrange
        var request = new AssociationRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "test-key",
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );
        request.MarkAsPaid();
        
        _mockPaymentRepository.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PaymentTransaction, bool>>>())
            .Returns(true);

        // Act
        await _membershipManager.ApproveRequestAsync(request);

        // Assert
        request.Status.ShouldBe(MembershipRequestStatus.Approved);
    }

    [Fact]
    public async Task ApproveRequestAsync_Should_Throw_When_No_Payment()
    {
        // Arrange
        var request = new AssociationRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "test-key",
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );
        request.MarkAsPaid();
        
        _mockPaymentRepository.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PaymentTransaction, bool>>>())
            .Returns(false);

        // Act & Assert
        await Should.ThrowAsync<BusinessException>(
            async () => await _membershipManager.ApproveRequestAsync(request)
        );
    }

    #endregion

    #region ProcessPayment Tests

    [Fact]
    public void ProcessPayment_Should_Mark_As_Paid_When_Valid()
    {
        // Arrange
        var alumniId = Guid.NewGuid();
        var request = new AssociationRequest(
            Guid.NewGuid(),
            alumniId,
            Guid.NewGuid(),
            "test-key",
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );

        // Act
        _membershipManager.ProcessPayment(request, alumniId);

        // Assert
        request.Status.ShouldBe(MembershipRequestStatus.Paid);
    }

    [Fact]
    public void ProcessPayment_Should_Throw_When_Unauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var request = new AssociationRequest(
            Guid.NewGuid(),
            ownerId,
            Guid.NewGuid(),
            "test-key",
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => 
            _membershipManager.ProcessPayment(request, attackerId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Membership.UnauthorizedPayment);
        exception.Data["RequestOwnerId"].ShouldBe(ownerId);
        exception.Data["CurrentUserId"].ShouldBe(attackerId);
    }

    [Fact]
    public void ProcessPayment_Should_Throw_When_Already_Paid()
    {
        // Arrange
        var alumniId = Guid.NewGuid();
        var request = new AssociationRequest(
            Guid.NewGuid(),
            alumniId,
            Guid.NewGuid(),
            "test-key",
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddYears(1),
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );
        request.MarkAsPaid(); // Already paid

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => 
            _membershipManager.ProcessPayment(request, alumniId)
        );

        exception.Code.ShouldBe(AlumniDomainErrorCodes.Membership.RequestNotPayable);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Full_Membership_Workflow_Should_Work_EndToEnd()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var alumniId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var idempotencyKey = "workflow-test-key";
        var branchId = Guid.NewGuid();
        
        var validFee = new SubscriptionFee(
            feeId,
            "2024 Membership",
            100m,
            2024,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20)
        );
        
        _mockFeeRepository.GetAsync(feeId, Arg.Any<bool>())
            .Returns(validFee);
        
        _mockRequestRepository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<AssociationRequest, bool>>>())
            .Returns(new System.Collections.Generic.List<AssociationRequest>());
        
        _mockPaymentRepository.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<PaymentTransaction, bool>>>())
            .Returns(true);

        // Act 1: Create Request
        var request = await _membershipManager.CreateMembershipRequestAsync(
            requestId,
            alumniId,
            feeId,
            idempotencyKey,
            branchId,
            2024,
            MemDeliveryMethod.OfficePickup,
            0,
            0, // UsedWalletAmount
            0, // RemainingAmount
            null
        );
        request.Status.ShouldBe(MembershipRequestStatus.Pending);

        // Act 2: Process Payment
        _membershipManager.ProcessPayment(request, alumniId);
        request.Status.ShouldBe(MembershipRequestStatus.Paid);

        // Act 3: Approve Request
        await _membershipManager.ApproveRequestAsync(request);
        request.Status.ShouldBe(MembershipRequestStatus.Approved);
    }

    #endregion
}
