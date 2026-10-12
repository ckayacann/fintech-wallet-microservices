using Auth.API.Data;
using Auth.API.DTOs;
using Auth.API.Models;
using Auth.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(AuthDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email.ToLower()))
        {
            return BadRequest(new { message = "Bu e-posta adresi zaten kullanımda." });
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Email = dto.Email.ToLower(),
            PasswordHash = passwordHash,
            FullName = dto.FullName,
            Role = dto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponseDto(token, user.Email, user.FullName, user.Role));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "E-posta veya şifre hatalı." });
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponseDto(token, user.Email, user.FullName, user.Role));
    }
}