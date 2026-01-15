
using System;
using System.Collections.Generic;

namespace Informatique.Alumni.Career;

public class CareerEmailJobArgs
{
    public Guid ServiceId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string> AttachmentBlobNames { get; set; } = new();
    public Guid SenderUserId { get; set; }
}
