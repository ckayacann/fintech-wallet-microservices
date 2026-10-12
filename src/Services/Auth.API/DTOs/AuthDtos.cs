using System.ComponentModel.DataAnnotations;

namespace Auth.API.DTOs;

public record RegisterDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string FullName,
    string Role = "Customer"
);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    string Email,
    string FullName,
    string Role
);