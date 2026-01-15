using System;

namespace Informatique.Alumni.Infrastructure.Email;

public class EmailJobArgs
{
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? TemplateName { get; set; }
    public object? TemplateModel { get; set; } // Will need serialization/handling if complex
    
    // Simple constructor for serialization
    public EmailJobArgs() { }
}
