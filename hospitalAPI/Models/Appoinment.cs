namespace hospitalAPI.Models;

public class Appoinment
{
    public int id { get; set; }

    public int clientId { get; set; }
    public Client client { get; set; }
    public int doctorId { get; set; }
    public Doctor doctor { get; set; }
    public int nurseId { get; set; }
    public Nurse nurse { get; set; }

    public DateTime ApointmentDate { get; set; }

    public string status { get; set; } = "Scheduled";


}
