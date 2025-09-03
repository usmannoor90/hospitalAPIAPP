namespace hospitalAPI.Models;

public class MedicalBilling
{
    public int id { get; set; }
    public int clientId { get; set; }
    public Client client { get; set; }


    public decimal Amount { get; set; }
    public DateTime DateIssued { get; set; }
    public DateTime DatePaid { get; set; }
    public bool IsPaid { get; set; }

}
