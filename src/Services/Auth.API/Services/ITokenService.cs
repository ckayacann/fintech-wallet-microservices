using Auth.API.Models;

namespace Auth.API.Services;

public interface ITokenService
{
    string CreateToken(User user);
}