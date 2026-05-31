using Microsoft.AspNetCore.Identity;

namespace PhoneStore.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LockedAt { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal WalletBalance { get; set; }
    public List<WalletTransaction> WalletTransactions { get; set; } = new();
}
