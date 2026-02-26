namespace Informatique.Alumni.Membership;

public class EligibilityCheckDto
{
    public string CheckName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pass";       // "Pass", "Fail", "Warning"
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = "check";         // "check", "x", "alert-triangle"
}
