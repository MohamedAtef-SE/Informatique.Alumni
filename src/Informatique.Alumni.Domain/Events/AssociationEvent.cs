using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Informatique.Alumni.Events;

public class AssociationEvent : FullAuditedAggregateRoot<Guid>
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public bool IsPublished { get; private set; }
    
    private readonly List<EventAgendaItem> _agenda = new();
    public IReadOnlyCollection<EventAgendaItem> Agenda => _agenda.AsReadOnly();

    private AssociationEvent()
    {
    }

    public AssociationEvent(Guid id, string title, string description, DateTime date, string location, int capacity)
        : base(id)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), EventConsts.MaxTitleLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), EventConsts.MaxDescriptionLength);
        Location = Check.NotNullOrWhiteSpace(location, nameof(location), EventConsts.MaxLocationLength);
        
        if (capacity <= 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.InvalidCapacity)
                .WithData("Capacity", capacity);
        }
        
        Capacity = capacity;
        Date = date;
        IsPublished = false;
    }

    public void Update(string title, string description, DateTime date, string location, int capacity)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), EventConsts.MaxTitleLength);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), EventConsts.MaxDescriptionLength);
        Location = Check.NotNullOrWhiteSpace(location, nameof(location), EventConsts.MaxLocationLength);
        
        if (capacity <= 0)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.InvalidCapacity)
                .WithData("Capacity", capacity);
        }
        
        Capacity = capacity;
        Date = date;
    }

    public void Publish()
    {
        if (IsPublished)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.AlreadyPublished);
        }
        
        IsPublished = true;
    }

    public void Unpublish()
    {
        if (!IsPublished)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.NotPublished);
        }
        
        IsPublished = false;
    }

    public bool CanRegister(int currentRegistrationCount)
    {
        if (!IsPublished)
        {
            return false;
        }
        
        if (currentRegistrationCount >= Capacity)
        {
            return false;
        }
        
        return true;
    }

    public void AddAgendaItem(Guid id, string title, DateTime startTime, DateTime endTime, string speaker)
    {
        // Validate agenda item times fall within event date range
        if (startTime.Date != Date.Date)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.AgendaItemOutsideEventDate)
                .WithData("StartTime", startTime)
                .WithData("EventDate", Date);
        }
        
        _agenda.Add(new EventAgendaItem(id, Id, title, startTime, endTime, speaker));
    }

    public void RemoveAgendaItem(Guid agendaItemId)
    {
        var item = _agenda.FirstOrDefault(x => x.Id == agendaItemId);
        if (item == null)
        {
            throw new BusinessException(AlumniDomainErrorCodes.AssociationEvent.AgendaItemNotFound)
                .WithData("AgendaItemId", agendaItemId);
        }
        
        _agenda.Remove(item);
    }
}

