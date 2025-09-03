using hospitalAPI.EFData;
using hospitalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hospitalAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Policy = "clientsOnly")]
[Authorize(Policy = "clientsOrAdmins")] //for testign only
//[Authorize(Policy = "adminOnly")]
[ApiExplorerSettings(GroupName = "clients")]

public class ClientsController : ControllerBase
{
    private readonly EFDateContext db;

    public ClientsController(EFDateContext db)
    {
        this.db = db;
    }

    [HttpGet("{id}", Name = "GetClient")]
    public async Task<IActionResult> GetClient(int id)
    {
        var client = await db.Clients
            .Include(c => c.User)
            .ThenInclude(u => u.Role)
            .Include(c => c.appoinments)
                .ThenInclude(a => a.doctor)
            .Include(c => c.medicalRecords)
            .Include(c => c.medicalBillings)
            .FirstOrDefaultAsync(c => c.id == id);

        if (client == null)
            return NotFound();

        return Ok(client);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst("userId")?.Value; // or ClaimTypes.NameIdentifier
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var client = await db.Clients
            .Include(c => c.User)
            .Include(c => c.appoinments)
            .Include(c => c.medicalRecords)
            .FirstOrDefaultAsync(c => c.userId == userId);

        if (client == null)
            return NotFound("No client profile linked to your account.");

        return Ok(client);
    }

    [HttpGet("{id}/appointments")]
    public async Task<IActionResult> GetAppointments(int id)
    {
        var exists = await db.Clients.AnyAsync(c => c.id == id);
        if (!exists) return NotFound("Client not found.");

        var appointments = await db.Appoinments
            .Where(a => a.clientId == id)
            .Include(a => a.doctor)
                .ThenInclude(d => d.id)
            .Include(a => a.nurse)
                .ThenInclude(n => n.id)
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("{id}/medical-records")]
    public async Task<IActionResult> GetMedicalRecords(int id)
    {
        var clientExists = await db.Clients.AnyAsync(c => c.id == id);
        if (!clientExists) return NotFound();

        var records = await db.MedicalRecords
            .Where(mr => mr.client.id == id)
            //.Include(mr => mr.)
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("{id}/bills")]
    public async Task<IActionResult> GetBills(int id)
    {
        var bills = await db.MedicalBillings
            .Where(b => b.clientId == id)
            .ToListAsync();

        return Ok(bills);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto)
    {
        var client = await db.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.id == id);

        if (client == null) return NotFound();

        // Optional: authorize – only self or admin can edit
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out int currentUserId) ||
            (client.userId != currentUserId && !User.IsInRole("Admin")))
            return Forbid();

        // Update fields
        client.User.phone = dto.Phone ?? client.User.phone;
        client.User.email = dto.Email ?? client.User.email;
        client.User.city = dto.City ?? client.User.city;
        client.User.DateOfBirth = dto.DateOfBirth ?? client.User.DateOfBirth;
        client.User.gender = dto.Gender ?? client.User.gender;

        await db.SaveChangesAsync();

        return Ok(client);
    }

    // DTO to avoid overposting

    [HttpPatch("me")]

    public async Task<IActionResult> PatchMyProfile([FromBody] JsonPatchDocument<User> patchDoc)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var client = await db.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.userId == userId);

        if (client == null) return NotFound();

        patchDoc.ApplyTo(client.User, error =>
        {
            ModelState.AddModelError(error.AffectedObject?.ToString() ?? string.Empty, error.ErrorMessage);
        });

        if (!TryValidateModel(client.User))
            return BadRequest(ModelState);

        await db.SaveChangesAsync();
        return Ok(client.User);
    }
    [HttpPost("{id}/appointments")]
    public async Task<IActionResult> BookAppointment(int id, [FromBody] BookAppointmentDto dto)
    {
        var client = await db.Clients.AnyAsync(c => c.id == id);
        if (!client) return NotFound("Client not found");

        var doctor = await db.Doctors.AnyAsync(d => d.id == dto.DoctorId);
        var nurse = await db.Nurses.AnyAsync(n => n.id == dto.NurseId);

        if (!doctor || !nurse) return BadRequest("Invalid doctor or nurse.");

        var appointment = new Appoinment
        {
            clientId = id,
            doctorId = dto.DoctorId,
            nurseId = dto.NurseId,
            ApointmentDate = dto.AppointmentDate,
            status = "Scheduled"
        };

        db.Appoinments.Add(appointment);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAppointments), new { id }, appointment);
    }


}

public class UpdateClientDto
{
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}


public class BookAppointmentDto
{
    public int DoctorId { get; set; }
    public int NurseId { get; set; }
    public DateTime AppointmentDate { get; set; }
}