using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Informatique.Alumni.Trips;

/// <summary>
/// Master data entity for trip room types.
/// DDD: Entity with business logic for validation.
/// Clean Code: Encapsulation with private setters.
/// </summary>
public class TripRoomType : Entity<Guid>
{
    /// <summary>
    /// Arabic name of the room type.
    /// Must be unique across all room types.
    /// </summary>
    public string NameAr { get; private set; } = null!;
    
    /// <summary>
    /// English name of the room type.
    /// Must be unique across all room types.
    /// </summary>
    public string NameEn { get; private set; } = null!;
    
    /// <summary>
    /// Maximum capacity for this room type.
    /// Example: Single = 1, Double = 2, Suite = 4
    /// </summary>
    public int Capacity { get; private set; }

    // EF Core constructor
    private TripRoomType()
    {
    }

    /// <summary>
    /// Constructor for creating a new room type.
    /// SRP: Constructor focuses on initialization only.
    /// </summary>
    public TripRoomType(
        Guid id,
        string nameAr,
        string nameEn,
        int capacity) : base(id)
    {
        SetNames(nameAr, nameEn);
        SetCapacity(capacity);
    }

    /// <summary>
    /// Updates the room type names.
    /// Clean Code: Validation in dedicated method.
    /// </summary>
    public void SetNames(string nameAr, string nameEn)
    {
        NameAr = Check.NotNullOrWhiteSpace(nameAr, nameof(nameAr), TripConsts.MaxRoomTypeNameLength);
        NameEn = Check.NotNullOrWhiteSpace(nameEn, nameof(nameEn), TripConsts.MaxRoomTypeNameLength);
    }

    /// <summary>
    /// Sets the room capacity.
    /// Business Rule: Capacity must be positive.
    /// </summary>
    public void SetCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Room capacity must be positive", nameof(capacity));
            
        Capacity = capacity;
    }
}
