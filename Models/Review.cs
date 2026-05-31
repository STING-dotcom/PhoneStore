namespace PhoneStore.Models;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ReviewerName { get; set; } = "Khách Hàng";
    public int? ReviewerUserId { get; set; }
    public User? ReviewerUser { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
