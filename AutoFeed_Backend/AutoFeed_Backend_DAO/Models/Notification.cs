using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoFeed_Backend_DAO.Models;

[Table("Notification")]
public class Notification
{
    [Key]
    [Column("notificationID")]
    public int NotificationId { get; set; }

    [Required]
    [Column("userID")]
    public int UserId { get; set; }

    [Required]
    [Column("type")]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [Column("title")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Required]
    [Column("isRead")]
    public bool IsRead { get; set; } = false;

    [Column("relatedID")]
    public int? RelatedId { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
