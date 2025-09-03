namespace hospitalAPI.Models;

public class Role
{
    public int id { get; set; }
    public RoleType RoleName { get; set; } = RoleType.Client;
    public ICollection<User> Users { get; set; } = new List<User>();
}

public enum RoleType
{
    Admin = 1,
    Doctor = 2,
    Nurse = 3,
    Client = 4
}