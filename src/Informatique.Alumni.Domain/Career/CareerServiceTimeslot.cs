using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

namespace Informatique.Alumni.Career;

public class CareerServiceTimeslot : FullAuditedEntity<Guid>
{
    public Guid CareerServiceId { get; private set; }
    public DateTime Date { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string LecturerName { get; private set; } = string.Empty;
    public string Room { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public int CurrentCount { get; private set; }

    private CareerServiceTimeslot() { }

    public CareerServiceTimeslot(
        Guid id, 
        Guid careerServiceId, 
        DateTime date, 
        TimeSpan startTime, 
        TimeSpan endTime, 
        string lecturerName,
        string room,
        string address,
        int capacity)
        : base(id)
    {
        CareerServiceId = careerServiceId;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        SetDetails(lecturerName, room, address);
        Capacity = capacity;
        CurrentCount = 0;
    }

    public void SetDetails(string lecturerName, string room, string address)
    {
        LecturerName = Check.NotNullOrWhiteSpace(lecturerName, nameof(lecturerName));
        Room = Check.NotNullOrWhiteSpace(room, nameof(room));
        Address = address; // Address can be optional or part of room
    }

    public void IncrementCount()
    {
        if (CurrentCount >= Capacity)
        {
            throw new BusinessException("Career:TimeslotFull");
        }
        CurrentCount++;
    }

    public void DecrementCount()
    {
        if (CurrentCount > 0)
        {
            CurrentCount--;
        }
    }
}
