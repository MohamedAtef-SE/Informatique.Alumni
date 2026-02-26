using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Informatique.Alumni.Syndicates;
using Volo.Abp.Content;

using Informatique.Alumni.Profiles;


[Authorize]
public class SyndicateAppService : AlumniAppService, ISyndicateAppService
{
    private readonly IRepository<Syndicate, Guid> _syndicateRepository;
    private readonly IRepository<SyndicateSubscription, Guid> _subscriptionRepository;
    private readonly SyndicateManager _syndicateManager;
    private readonly IBlobContainer<SyndicateBlobContainer> _blobContainer;
    private readonly AlumniApplicationMappers _alumniMappers;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;
    private readonly Informatique.Alumni.Payment.IPaymentAppService _paymentAppService;

    public SyndicateAppService(
        IRepository<Syndicate, Guid> syndicateRepository,
        IRepository<SyndicateSubscription, Guid> subscriptionRepository,
        SyndicateManager syndicateManager,
        IBlobContainer<SyndicateBlobContainer> blobContainer,
        AlumniApplicationMappers alumniMappers,
        IRepository<AlumniProfile, Guid> alumniProfileRepository,
        Informatique.Alumni.Payment.IPaymentAppService paymentAppService)
    {
        _syndicateRepository = syndicateRepository;
        _subscriptionRepository = subscriptionRepository;
        _syndicateManager = syndicateManager;
        _blobContainer = blobContainer;
        _alumniMappers = alumniMappers;
        _alumniProfileRepository = alumniProfileRepository;
        _paymentAppService = paymentAppService;
    }

    public async Task<List<SyndicateDto>> GetSyndicatesAsync()
    {
        var list = await _syndicateRepository.GetListAsync();
        return _alumniMappers.MapToDtos(list);
    }

    [Authorize(AlumniPermissions.Syndicates.Manage)]
    public async Task<SyndicateDto> CreateSyndicateAsync(CreateUpdateSyndicateDto input)
    {
        var syndicate = new Syndicate(GuidGenerator.Create(), input.Name, input.Description, input.Requirements, input.Fee);
        await _syndicateRepository.InsertAsync(syndicate);
        return _alumniMappers.MapToDto(syndicate);
    }

    [Authorize]
    public async Task<SyndicateSubscriptionDto> ApplyAsync(ApplySyndicateDto input)
    {
        var alumniId = CurrentUser.GetId();
        var existingSubscription = await _subscriptionRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId && x.SyndicateId == input.SyndicateId);
        
        if (existingSubscription != null)
        {
            // Resume if Draft
            if (existingSubscription.Status == SyndicateStatus.Draft)
            {
                return _alumniMappers.MapToDto(existingSubscription);
            }
            // Block if already submitted
            throw new UserFriendlyException("You have already applied for this syndicate.");
        }

        var syndicate = await _syndicateRepository.GetAsync(input.SyndicateId);

        var subscription = new SyndicateSubscription(GuidGenerator.Create(), alumniId, input.SyndicateId, syndicate.Fee, input.DeliveryMethod);
        await _subscriptionRepository.InsertAsync(subscription);
        return _alumniMappers.MapToDto(subscription);
    }

    [Authorize]
    public async Task<SyndicateSubscriptionDto> GetMySubscriptionAsync(Guid syndicateId)
    {
        var alumniId = CurrentUser.GetId();
        var subscription = await _subscriptionRepository.FirstOrDefaultAsync(x => x.AlumniId == alumniId && x.SyndicateId == syndicateId);
        if (subscription == null) throw new EntityNotFoundException();
        
        await _subscriptionRepository.EnsureCollectionLoadedAsync(subscription, x => x.Documents);
        var dto = _alumniMappers.MapToDto(subscription);
        
        var syndicate = await _syndicateRepository.GetAsync(syndicateId);
        dto.SyndicateName = syndicate.Name;
        
        return dto;
    }

    [Authorize]
    public async Task<List<SyndicateSubscriptionDto>> GetMyApplicationsAsync()
    {
        var alumniId = CurrentUser.GetId();
        var query = await _subscriptionRepository.WithDetailsAsync(x => x.Documents);
        var subList = await AsyncExecuter.ToListAsync(query.Where(x => x.AlumniId == alumniId));
        var dtos = _alumniMappers.MapToDtos(subList);

        if (dtos.Any())
        {
            var syndicateIds = dtos.Select(x => x.SyndicateId).Distinct().ToArray();
            var syndicates = await _syndicateRepository.GetListAsync(x => syndicateIds.Contains(x.Id));
            var syndicateDict = syndicates.ToDictionary(x => x.Id, x => x.Name);

            foreach (var dto in dtos)
            {
                if (syndicateDict.TryGetValue(dto.SyndicateId, out var name))
                {
                    dto.SyndicateName = name;
                }
            }
        }

        return dtos;
    }

    [HttpPost("/api/app/syndicate-payments/{subscriptionId}")]
    [Authorize]
    public async Task<SyndicateSubscriptionDto> PayAsync(Guid subscriptionId)
    {
        var subscription = await _subscriptionRepository.GetAsync(subscriptionId);
        if (subscription.AlumniId != CurrentUser.GetId()) throw new UnauthorizedAccessException();

        if (subscription.PaymentStatus == Informatique.Alumni.Syndicates.PaymentStatus.Paid) 
        {
             var paidDto = _alumniMappers.MapToDto(subscription);
             var paidSyndicate = await _syndicateRepository.GetAsync(subscription.SyndicateId);
             paidDto.SyndicateName = paidSyndicate.Name;
             return paidDto;
        }

        // Call Payment Service
        var paymentResult = await _paymentAppService.CheckoutAsync(new Informatique.Alumni.Payment.CheckoutDto
        {
            OrderId = subscription.Id,
            Amount = subscription.FeeAmount,
            Currency = "EGP", 
            Description = $"Syndicate Application Fee: {subscription.Id}"
        });

        if (paymentResult.Status == Informatique.Alumni.Payment.PaymentStatus.Completed)
        {
             subscription.ConfirmPayment(paymentResult.Amount, paymentResult.GatewayTransactionId, Informatique.Alumni.Payment.PaymentGatewayType.Mock);
             await _subscriptionRepository.UpdateAsync(subscription);
        }

        var dto = _alumniMappers.MapToDto(subscription);
        var syndicate = await _syndicateRepository.GetAsync(subscription.SyndicateId);
        dto.SyndicateName = syndicate.Name;

        return dto;
    }

    [Authorize]
    public async Task<SyndicateSubscriptionDto> GetApplicationDetailsAsync(Guid subscriptionId)
    {
        var subscription = await _subscriptionRepository.GetAsync(subscriptionId);
        if (subscription.AlumniId != CurrentUser.GetId()) throw new UnauthorizedAccessException();

        var dto = _alumniMappers.MapToDto(subscription);
        var syndicate = await _syndicateRepository.GetAsync(subscription.SyndicateId);
        dto.SyndicateName = syndicate.Name;

        return dto;
    }

    [HttpPost("/api/app/syndicate-uploads/{subscriptionId}")]
    [Authorize]
    public async Task UploadDocumentAsync(Guid subscriptionId, [FromBody] UploadSyndicateDocDto input)
    {
        var subscription = await _subscriptionRepository.GetAsync(subscriptionId);
        if (subscription.AlumniId != CurrentUser.GetId()) throw new UserFriendlyException("You do not have access to this subscription.");
        
        if (subscription.Status != SyndicateStatus.Draft && 
            subscription.Status != SyndicateStatus.Pending && 
            subscription.Status != SyndicateStatus.Reviewing)
        {
            throw new UserFriendlyException("Cannot upload documents in current status.");
        }

        var blobName = $"syndicate_doc_{subscriptionId}_{GuidGenerator.Create()}_{input.FileName}";
        await _blobContainer.SaveAsync(blobName, input.FileContent);
        
        await _subscriptionRepository.EnsureCollectionLoadedAsync(subscription, x => x.Documents);
        subscription.AddDocument(GuidGenerator.Create(), input.RequirementName, blobName);
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    [HttpGet("/api/app/syndicate-uploads/{subscriptionId}/{documentId}")]
    [Authorize]
    public async Task<IRemoteStreamContent> GetDocumentAsync(Guid subscriptionId, Guid documentId)
    {
        var subscription = await _subscriptionRepository.GetAsync(subscriptionId);
        if (subscription.AlumniId != CurrentUser.GetId()) throw new UnauthorizedAccessException();

        await _subscriptionRepository.EnsureCollectionLoadedAsync(subscription, x => x.Documents);
        var document = subscription.Documents.FirstOrDefault(x => x.Id == documentId);
        
        if (document == null) throw new EntityNotFoundException("Document not found");

        var content = await _blobContainer.GetAllBytesOrNullAsync(document.FileBlobName);
        if (content == null) throw new EntityNotFoundException("File content not found");

        return new RemoteStreamContent(new System.IO.MemoryStream(content), document.FileBlobName, "application/octet-stream");
    }

    [Authorize(AlumniPermissions.Syndicates.Manage)]
    public async Task BatchUpdateStatusAsync(BatchUpdateStatusDto input)
    {
        // 1. Eager load Documents
        var query = await _subscriptionRepository.WithDetailsAsync(x => x.Documents);
        var subscriptions = await AsyncExecuter.ToListAsync(query.Where(x => input.SubscriptionIds.Contains(x.Id)));

        // 2. Bulk load Syndicates
        var syndicateIds = subscriptions.Select(x => x.SyndicateId).Distinct().ToArray();
        var syndicates = await _syndicateRepository.GetListAsync(x => syndicateIds.Contains(x.Id));
        var syndicateDict = syndicates.ToDictionary(x => x.Id);
        
        foreach (var subscription in subscriptions)
        {
            if (!syndicateDict.TryGetValue(subscription.SyndicateId, out var syndicate)) continue;

            // Documents are already loaded via WithDetailsAsync

            switch (input.NewStatus)
            {
                case SyndicateStatus.Reviewing:
                    subscription.MarkAsInProgress();
                    break;
                case SyndicateStatus.SentToSyndicate:
                    _syndicateManager.VerifyRequirementCompletion(subscription, syndicate);
                    // Revisit logic: if state transition is needed, add method to entity.
                    // For now, allow transition if valid.
                    break;
                case SyndicateStatus.CardReady:
                    subscription.MarkAsReadyForPickup();
                    break;
                case SyndicateStatus.Rejected:
                    subscription.Reject(input.AdminNotes ?? "Rejected by admin");
                    break;
            }
        }
        
        await _subscriptionRepository.UpdateManyAsync(subscriptions);
    }

    [Authorize(AlumniPermissions.Syndicates.Manage)]
    public async Task<SyndicateSubscriptionDto> CreateRequestByEmployeeAsync(CreateSyndicateRequestDto input)
    {
        var currentBranchId = GetCurrentUserBranchId();
        var alumni = await _alumniProfileRepository.GetAsync(input.TargetAlumniId);
        
        if (alumni.BranchId != currentBranchId)
        {
            throw new UserFriendlyException("You can only manage requests for Alumni in your branch.");
        }

        // Use new constructor
        var subscription = new SyndicateSubscription(GuidGenerator.Create(), input.TargetAlumniId, input.SyndicateId, input.FeeAmount);
        // Status is Pending by default. PaymentStatus is NotPaid by default.

        await _subscriptionRepository.InsertAsync(subscription);
        return _alumniMappers.MapToDto(subscription);
    }

    [Authorize(AlumniPermissions.Syndicates.Manage)]
    public async Task<SyndicateSubscriptionDto> UpdateRequestStatusAsync(Guid id, SyndicateRequestStatus newStatus)
    {
        var subscription = await _subscriptionRepository.GetAsync(id);
        
        var alumni = await _alumniProfileRepository.GetAsync(subscription.AlumniId);
        var currentBranchId = GetCurrentUserBranchId();
        if (alumni.BranchId != currentBranchId)
        {
             throw new UserFriendlyException("You can only manage requests for Alumni in your branch.");
        }

        var targetStatus = (SyndicateStatus)newStatus;
        
        _syndicateManager.ValidateStatusTransition(subscription.Status, targetStatus, subscription.PaymentStatus == PaymentStatus.Paid);

        // Switch to call methods
        switch (targetStatus)
        {
            case SyndicateStatus.Reviewing:
                subscription.MarkAsInProgress();
                break;
            case SyndicateStatus.CardReady:
                subscription.MarkAsReadyForPickup();
                break;
            case SyndicateStatus.Received:
                subscription.MarkAsReceived();
                break;
            default:
                // Handle others or throw
                break;
        }

        await _subscriptionRepository.UpdateAsync(subscription);
        
        return _alumniMappers.MapToDto(subscription);
    }

    [Authorize(AlumniPermissions.Syndicates.Manage)]
    public async Task<PagedResultDto<SyndicateSubscriptionDto>> GetListAsync(SyndicateRequestFilterDto input)
    {
        var currentBranchId = GetCurrentUserBranchId();
        // Force Filter
        input.BranchId = currentBranchId;

        // Query with Join to filter by Alumni.BranchId
        var query = await _subscriptionRepository.GetQueryableAsync();
        var alumniQuery = await _alumniProfileRepository.GetQueryableAsync();

        var combinedQuery = from sub in query
                            join al in alumniQuery on sub.AlumniId equals al.Id
                            where al.BranchId == currentBranchId
                            select new { Sub = sub, Al = al };

        // Date Range
        combinedQuery = combinedQuery.Where(x => x.Sub.CreationTime >= input.FromDate && x.Sub.CreationTime <= input.ToDate);

        // Optional Filters
        if (input.Status.HasValue)
        {
            combinedQuery = combinedQuery.Where(x => x.Sub.Status == input.Status.Value);
        }
        if (input.PaymentStatus.HasValue)
        {
            combinedQuery = combinedQuery.Where(x => x.Sub.PaymentStatus == input.PaymentStatus.Value);
        }
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            // Filter by Alumni Name/NationalId/etc?
            // Assuming AlumniProfile has Name/NationalId, but Name is on IdentityUser.
            // AlumniProfile has NationalId, MobileNumber.
            combinedQuery = combinedQuery.Where(x => x.Al.NationalId.Contains(input.Filter) || x.Al.MobileNumber.Contains(input.Filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(combinedQuery);
        
        var results = await AsyncExecuter.ToListAsync(
            combinedQuery
                .OrderByDescending(x => x.Sub.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = _alumniMappers.MapToDtos(results.Select(x => x.Sub).ToList());
        return new PagedResultDto<SyndicateSubscriptionDto>(totalCount, dtos);
    }

    private Guid GetCurrentUserBranchId()
    {
        var claim = CurrentUser.FindClaim("BranchId");
        if (claim == null || !Guid.TryParse(claim.Value, out var guid))
        {
            // For dev/test, maybe return a default or throw?
            // "Rule: An Employee can ONLY view..." implies strictness.
            // If I am admin/host, maybe skip?
            // But requirement is User Story: Employee.
            // I'll throw exception if missing.
            throw new UserFriendlyException("Current user does not have an assigned branch.");
        }
        return guid;
    }
    }


