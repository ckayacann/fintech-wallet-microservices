using System.ComponentModel.DataAnnotations;

namespace Account.API.Models;

public class AccountModel
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; } // Auth.API'deki User.Id ile eşleşir

    [Required]
    public string AccountNumber { get; set; } = string.Empty; // TR123456789

    public decimal Balance { get; set; } = 0;

    public string Currency { get; set; } = "TRY";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}