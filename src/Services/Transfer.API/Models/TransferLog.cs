namespace Transfer.API.Models;

public class TransferLog
{
    public Guid Id { get; set; }
    public string FromAccount { get; set; } = string.Empty;
    public string ToAccount { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}