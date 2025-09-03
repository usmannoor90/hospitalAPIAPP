namespace hospitalAPI.Models;

public class Doctor

{
    public int id { get; set; }
    public int userId { get; set; }
    public User user { get; set; }

    public string specialization { get; set; }

    public string licenseNumber { get; set; }

    public ICollection<Appoinment> appoinments { get; set; }

}
