using System;
using Informatique.Alumni.Profiles;
using Volo.Abp.Application.Dtos;

namespace Informatique.Alumni.Admin;

/// <summary>
/// Full admin DTO with all fields including sensitive data.
/// </summary>
public class AlumniAdminDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    // Profile
    public string JobTitle { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Company { get; set; }
    public string? FacebookUrl { get; set; }
    public string? LinkedinUrl { get; set; }

    // Admin Fields
    public AlumniStatus Status { get; set; }
    public bool IsVip { get; set; }
    public bool IsNotable { get; set; }
    public IdCardStatus IdCardStatus { get; set; }
    public string? RejectionReason { get; set; }
    public decimal WalletBalance { get; set; }
    public int ViewCount { get; set; }
    public bool ShowInDirectory { get; set; }

    // Audit
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

/// <summary>
/// Lightweight list DTO for admin grids.
/// </summary>
public class AlumniAdminListDto : EntityDto<Guid>
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public AlumniStatus Status { get; set; }
    public bool IsVip { get; set; }
    public bool IsNotable { get; set; }
    public IdCardStatus IdCardStatus { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// Input DTO for filtering alumni list.
/// </summary>
public class AlumniAdminGetListInput : PagedAndSortedResultRequestDto
{
    public AlumniStatus? StatusFilter { get; set; }
    public string? Filter { get; set; }
}

/// <summary>
/// Input for rejecting an alumni.
/// </summary>
public class RejectAlumniInput
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Input for updating ID card status.
/// </summary>
public class UpdateIdCardStatusInput
{
    public IdCardStatus Status { get; set; }
}
