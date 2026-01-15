using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Informatique.Alumni.Communication;

public class SendMassMessageDto
{
    public List<Guid>? TargetUserIds { get; set; } // Null means all
    public string? FilterMajor { get; set; }
    public int? FilterYear { get; set; }
    
    [Required]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public bool SendAsEmail { get; set; } = true;
    public bool SendAsSms { get; set; } = false;
}
