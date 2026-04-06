using System;

namespace Informatique.Alumni.Import;

public class SendImportedAlumniWelcomeEmailJobArgs
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
}
