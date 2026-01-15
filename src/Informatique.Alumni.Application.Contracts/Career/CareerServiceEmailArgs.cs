using System;

namespace Informatique.Alumni.Career;

public class CareerServiceEmailArgs
{
    public Guid ServiceId { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Guid? SenderId { get; set; }
}
