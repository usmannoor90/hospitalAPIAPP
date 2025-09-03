namespace hospitalAPI.Models;

public class Client
{
    public int id { get; set; }
    //this is going to be a foriegn key for a user in a client's table.
    public int userId { get; set; }
    //for upper userId give me that user so only one
    public User User { get; set; }
    public ICollection<Appoinment> appoinments { get; set; }
    public ICollection<MedicalRecord> medicalRecords { get; set; }
    public ICollection<MedicalBilling> medicalBillings { get; set; }

}
