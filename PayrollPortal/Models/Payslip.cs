using System.ComponentModel.DataAnnotations;

public class Payslip
{
    public int Id { get; set; }

    [Required]
    public string? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }

    [Required]
    public string? Month { get; set; }  // e.g., "2025-05"

    [Required]
    public string? FileName { get; set; }

    [Required]
    public string? FilePath { get; set; }

    public DateTime UploadedDate { get; set; }
}
