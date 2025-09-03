namespace hospitalAPI.Models;

public class MedicalRecord
{
    public int id { get; set; }
    public int userId { get; set; }
    public Client client { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public DateTime RecordDate { get; set; } = DateTime.Now;
}
