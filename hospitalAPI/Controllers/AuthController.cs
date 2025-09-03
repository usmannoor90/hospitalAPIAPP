using hospitalAPI.EFData;
using hospitalAPI.Models;
using hospitalAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace hospitalAPI.Controllers;

[Route("api/auth")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly EFDateContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _config;

    public record LoginRequest(string Name, string Password);

    public record RegisterRequest(
        string Name,
        string Password,
        string Email,
        string? Phone,
        string? FullName,
        RoleType Role = RoleType.Client
    );

    public record AuthResponse(string Token, string Role);
    public AuthController(EFDateContext context, IPasswordHasher<User> passwordHasher, IConfiguration config)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CustomResponses<AuthResponse>.ValidationError(ModelState));


        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.name == request.Name);

        if (user == null)
            return Unauthorized(CustomResponses<AuthResponse>.Failure("Invalid credentials.", "AUTH_INVALID"));

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(CustomResponses<AuthResponse>.Failure("Invalid credentials.", "AUTH_INVALID"));

        // Optional: rehash if needed (e.g. algorithm improved)
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);

        var authResponse = new AuthResponse(token, user.Role.RoleName.ToString());

        return Ok(CustomResponses<AuthResponse>.Success(authResponse, "Login successful."));
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CustomResponses<AuthResponse>.ValidationError(ModelState));

        // Prevent duplicate usernames
        if (await _context.Users.AnyAsync(u => u.name == request.Name))
            return BadRequest(CustomResponses<AuthResponse>.Failure("Username already exists.", "AUTH_INVALID"));

        // Lookup role
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == request.Role)
            ?? await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == RoleType.Client);

        var user = new User
        {
            name = request.Name,
            email = request.Email,
            phone = request.Phone,
            FullName = request.FullName,
            password = request.Password, // just for testing not in production
            RoleId = role.id,
            Role = role
        };

        // Hash password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();


        // With this line:
        return Ok(CustomResponses<object>.Success(new { message = "User registered successfully." }));

    }

    private string GenerateJwtToken(User user)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing.");
        var issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing.");
        var audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()), // "sub": user ID
            new Claim(JwtRegisteredClaimNames.UniqueName, user.name),  // "unique_name"
            new Claim(JwtRegisteredClaimNames.Email, user.email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.RoleName.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(2), // 2 hours better than 60 mins
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// =====================
// DTOs (Place in Models or Contracts folder)
// =====================

