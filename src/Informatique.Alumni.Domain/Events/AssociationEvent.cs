using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Events;

public class AssociationEvent : FullAuditedAggregateRoot<Guid>
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty; // PlaceName
    public string? Address { get; private set; }
    public string? GoogleMapUrl { get; private set; }
    
    public bool HasFees { get; private set; }
    public decimal? FeeAmount { get; private set; }
    public DateTime LastSubscriptionDate { get; private set; }
    public bool IsPublished { get; private set; }

    public Guid? BranchId { get; private set; }

    // Navigation Properties
    private readonly List<EventTimeslot> _timeslots = new();
    public IReadOnlyCollection<EventTimeslot> Timeslots => _timeslots.AsReadOnly();

    private readonly List<EventParticipatingCompany> _participatingCompanies = new();
    public IReadOnlyCollection<EventParticipatingCompany> ParticipatingCompanies => _participatingCompanies.AsReadOnly();

    private readonly List<EventAgendaItem> _agendaItems = new();
    public IReadOnlyCollection<EventAgendaItem> AgendaItems => _agendaItems.AsReadOnly();
    
    // Legacy support or new agenda? keeping as is for safety if used elsewhere, but maybe removing if replaced by timeslots.
    // The requirement says "AssociationEvent has a collection of EventTimeslot". 
    // I will keep Agenda if it was in the original file to avoid breaking too much, 
    // but the prompt implies a meaningful change. 
    // Since I'm essentially rewriting it, I'll comment out Agenda if it conflicts, 
    // but the prompt didn't say to DELETE Agenda, just ADD Timeslots.
    // However, the "Event Structure & Scheduling" rule says "An Event is not just a single timestamp... AssociationEvent has a collection of EventTimeslot".
    // This likely replaces the single "Date" property.
    
    private AssociationEvent()
    {
    }

    public AssociationEvent(
        Guid id, 
        string nameAr, 
        string nameEn, 
        string code, 
        string description, 
        string location, 
        string? address, 
        string? googleMapUrl,
        bool hasFees,
        decimal? feeAmount,
        DateTime lastSubscriptionDate,
        Guid? branchId)
        : base(id)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
        Code = Check.NotNullOrWhiteSpace(code, nameof(code));
        Description = Check.NotNullOrWhiteSpace(description, nameof(description));
        Location = Check.NotNullOrWhiteSpace(location, nameof(location));
        Address = address;
        GoogleMapUrl = googleMapUrl;
        
        SetFees(hasFees, feeAmount);
        LastSubscriptionDate = lastSubscriptionDate;
        BranchId = branchId;
        IsPublished = false;
    }

    public void SetFees(bool hasFees, decimal? feeAmount)
    {
        if (hasFees)
        {
            if (!feeAmount.HasValue || feeAmount.Value <= 0)
            {
                 throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.InvalidFeeAmount);
            }
        }
        else
        {
            if (feeAmount.HasValue && feeAmount.Value != 0)
            {
                 throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.FeeAmountMustBeZero);
            }
        }

        HasFees = hasFees;
        FeeAmount = feeAmount;
    }

    public void AddTimeslot(Guid id, DateTime startTime, DateTime endTime, int capacity)
    {
        if (startTime >= endTime)
        {
             throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.InvalidTimeslotRange);
        }
        
        _timeslots.Add(new EventTimeslot(id, Id, startTime, endTime, capacity));
    }
    
    public void RemoveTimeslot(Guid timeslotId)
    {
        var item = _timeslots.FirstOrDefault(x => x.Id == timeslotId);
        if (item != null)
        {
            _timeslots.Remove(item);
        }
    }

    public void AddParticipatingCompany(Guid id, Guid companyId, Guid participationTypeId)
    {
        if (_participatingCompanies.Any(x => x.CompanyId == companyId))
        {
            return; // Already added
        }
        _participatingCompanies.Add(new EventParticipatingCompany(id, Id, companyId, participationTypeId));
    }
    
    public void RemoveParticipatingCompany(Guid companyId)
    {
         var item = _participatingCompanies.FirstOrDefault(x => x.CompanyId == companyId);
         if (item != null)
         {
             _participatingCompanies.Remove(item);
         }
    }

    public void AddAgendaItem(EventAgendaItem item)
    {
        Check.NotNull(item, nameof(item));
        _agendaItems.Add(item);
    }

    public void RemoveAgendaItem(Guid agendaItemId)
    {
        var item = _agendaItems.FirstOrDefault(x => x.Id == agendaItemId);
        if (item != null)
        {
            _agendaItems.Remove(item);
        }
    }

    public void ClearAgendaItems()
    {
        _agendaItems.Clear();
    }

    public void Update(
        string nameAr, 
        string nameEn, 
        string code, 
        string description, 
        string location, 
        string? address, 
        string? googleMapUrl,
        bool hasFees,
        decimal? feeAmount,
        DateTime lastSubscriptionDate,
        Guid? branchId)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr));
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn));
        Code = Check.NotNullOrWhiteSpace(code, nameof(code));
        Description = Check.NotNullOrWhiteSpace(description, nameof(description));
        Location = Check.NotNullOrWhiteSpace(location, nameof(location));
        Address = address;
        GoogleMapUrl = googleMapUrl;
        
        SetFees(hasFees, feeAmount);
        LastSubscriptionDate = lastSubscriptionDate;
        BranchId = branchId;
    }

    public void ClearTimeslots()
    {
        _timeslots.Clear();
    }

    public void ClearParticipatingCompanies()
    {
        _participatingCompanies.Clear();
    }

    public void Publish()
    {
        if (!_timeslots.Any())
        {
             throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.NoTimeslotsDefined);
        }
        IsPublished = true;
    }

    public void Unpublish()
    {
        IsPublished = false;
    }
    public Guid? ActivityTypeId { get; private set; }
    public ActivityType? ActivityType { get; private set; }

    public void SetActivityType(Guid? activityTypeId)
    {
        ActivityTypeId = activityTypeId;
    }
}

