using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Informatique.Alumni.Permissions;
using Informatique.Alumni.Syndicates;
using Informatique.Alumni.Profiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Microsoft.AspNetCore.Mvc;

namespace Informatique.Alumni.Admin;

[Authorize(AlumniPermissions.Admin.SyndicateManage)]
public class SyndicateAdminAppService : AlumniAppService, ISyndicateAdminAppService
{
    private readonly IRepository<SyndicateSubscription, Guid> _subscriptionRepository;
    private readonly IRepository<Syndicate, Guid> _syndicateRepository;
    private readonly IRepository<AlumniProfile, Guid> _alumniProfileRepository;
    private readonly IRepository<IdentityUser, Guid> _identityUserRepository;
    private readonly IBlobContainer<SyndicateBlobContainer> _blobContainer;

    public SyndicateAdminAppService(
        IRepository<SyndicateSubscription, Guid> subscriptionRepository,
        IRepository<Syndicate, Guid> syndicateRepository,
        IRepository<AlumniProfile, Guid> alumniProfileRepository,
        IRepository<IdentityUser, Guid> identityUserRepository,
        IBlobContainer<SyndicateBlobContainer> blobContainer)
    {
        _subscriptionRepository = subscriptionRepository;
        _syndicateRepository = syndicateRepository;
        _alumniProfileRepository = alumniProfileRepository;
        _identityUserRepository = identityUserRepository;
        _blobContainer = blobContainer;
    }

    public async Task<PagedResultDto<SyndicateSubscriptionAdminDto>> GetSubscriptionsAsync(SyndicateAdminGetListInput input)
    {
        var queryable = await _subscriptionRepository.WithDetailsAsync(x => x.Documents);

        if (input.StatusFilter.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.StatusFilter.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var subscriptions = await AsyncExecuter.ToListAsync(queryable);

        // Batch lookup syndicate names
        var syndicateIds = subscriptions.Select(s => s.SyndicateId).Distinct().ToList();
        var syndicates = await _syndicateRepository.GetListAsync(s => syndicateIds.Contains(s.Id));

        // Batch lookup alumni profiles and users
        // Note: s.AlumniId from the subscription is actually the IdentityUser Id.
        var userIds = subscriptions.Select(s => s.AlumniId).Distinct().ToList();
        var users = await _identityUserRepository.GetListAsync(u => userIds.Contains(u.Id));
        
        // Eager load Mobiles to correctly get the primary contact number
        var profilesQuery = await _alumniProfileRepository.WithDetailsAsync(x => x.Mobiles);
        var profiles = await AsyncExecuter.ToListAsync(profilesQuery.Where(p => userIds.Contains(p.UserId)));

        var items = subscriptions.Select(s =>
        {
            var syndicate = syndicates.FirstOrDefault(syn => syn.Id == s.SyndicateId);
            var user = users.FirstOrDefault(u => u.Id == s.AlumniId);
            var profile = profiles.FirstOrDefault(p => p.UserId == s.AlumniId);
            var fullName = user != null ? $"{user.Name} {user.Surname}".Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(fullName) && user != null) fullName = user.UserName;

            var primaryMobile = profile?.Mobiles?.FirstOrDefault(m => m.IsPrimary)?.MobileNumber 
                                ?? profile?.Mobiles?.FirstOrDefault()?.MobileNumber 
                                ?? profile?.MobileNumber 
                                ?? "—";

            return new SyndicateSubscriptionAdminDto
            {
                Id = s.Id,
                AlumniId = s.AlumniId,
                AlumniName = string.IsNullOrWhiteSpace(fullName) ? "—" : fullName,
                AlumniNationalId = profile?.NationalId ?? "—",
                AlumniMobile = primaryMobile,
                SyndicateId = s.SyndicateId,
                SyndicateName = syndicate?.Name ?? "—",
                Status = s.Status,
                FeeAmount = s.FeeAmount,
                PaymentStatus = s.PaymentStatus,
                DeliveryMethod = s.DeliveryMethod,
                AdminNotes = s.AdminNotes,
                CreationTime = s.CreationTime,
                Documents = s.Documents.Select(d => new SyndicateDocumentDto
                {
                    Id = d.Id,
                    RequirementName = d.RequirementName,
                    FileBlobName = d.FileBlobName
                }).ToList()
            };
        }).ToList();

        return new PagedResultDto<SyndicateSubscriptionAdminDto>(totalCount, items);
    }

    public async Task MarkAsInProgressAsync(Guid id)
    {
        var subscription = await _subscriptionRepository.GetAsync(id);
        
        if (subscription.PaymentStatus != PaymentStatus.Paid)
        {
            throw new Volo.Abp.UserFriendlyException("Cannot start processing until fees are paid.");
        }
        
        subscription.MarkAsInProgress();
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    public async Task MarkAsReadyForPickupAsync(Guid id)
    {
        var subscription = await _subscriptionRepository.GetAsync(id);
        subscription.MarkAsReadyForPickup();
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    public async Task MarkAsReceivedAsync(Guid id)
    {
        var subscription = await _subscriptionRepository.GetAsync(id);
        subscription.MarkAsReceived();
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    public async Task RejectAsync(Guid id, string reason)
    {
        var subscription = await _subscriptionRepository.GetAsync(id);
        subscription.Reject(reason);
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    [HttpGet("/api/app/syndicate-admin/document/{subscriptionId}/{documentId}")]
    [Authorize(AlumniPermissions.Admin.SyndicateManage)]
    public async Task<IRemoteStreamContent> GetDocumentAsync(Guid subscriptionId, Guid documentId)
    {
        var subscription = await _subscriptionRepository.GetAsync(subscriptionId);
        await _subscriptionRepository.EnsureCollectionLoadedAsync(subscription, x => x.Documents);
        var document = subscription.Documents.FirstOrDefault(x => x.Id == documentId);
        
        if (document == null) throw new Volo.Abp.Domain.Entities.EntityNotFoundException("Document not found");

        var content = await _blobContainer.GetAllBytesOrNullAsync(document.FileBlobName);
        if (content == null) throw new Volo.Abp.Domain.Entities.EntityNotFoundException("File content not found");

        // Simple MIME type guessing - usually PDFs or JPEGs
        var mimeType = document.FileBlobName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "application/pdf" : 
                       document.FileBlobName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || document.FileBlobName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ? "image/jpeg" :
                       document.FileBlobName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "application/octet-stream";

        return new RemoteStreamContent(new System.IO.MemoryStream(content), document.FileBlobName, mimeType);
    }
}
