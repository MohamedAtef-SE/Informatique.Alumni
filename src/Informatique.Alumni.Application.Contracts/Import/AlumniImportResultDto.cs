using System.Collections.Generic;

namespace Informatique.Alumni.Import;

public class AlumniImportResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> ErrorMessages { get; set; } = new List<string>();
}
