using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Informatique.Alumni.Events;

public class SendEventEmailInputDto
{
    [Required]
    public Guid EventId { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// Attachments to send with the email (Word, Excel, PowerPoint, Photos, Videos)
    /// </summary>
    public List<IRemoteStreamContent>? Attachments { get; set; }
}

public class EventEmailJobArgs
{
    public Guid EventId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string> AttachmentBlobNames { get; set; } = new();
    public Guid SenderUserId { get; set; }
}
