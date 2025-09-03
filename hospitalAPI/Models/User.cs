namespace hospitalAPI.Models;

public class User
{
    public int id { get; set; }
    public string name { get; set; }
    public string? FullName { get; set; } = string.Empty;
    public string password { get; set; }
    public string PasswordHash { get; set; }
    public string? email { get; set; }
    public string? gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? phone { get; set; }
    public string? city { get; set; }
    // Navigation
    public int? RoleId { get; set; }
    public Role Role { get; set; }

}
