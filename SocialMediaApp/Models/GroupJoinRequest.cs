using SocialMediaApp.Models;
using System.ComponentModel.DataAnnotations;

public class GroupJoinRequest
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }
    public string UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Proprietati de navigatie: 2
    public Group? Group { get; set; }
    public ApplicationUser? User { get; set; }
}
