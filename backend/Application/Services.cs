using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using Spindle.Backend.Domain;

namespace Spindle.Backend.Application;

public interface IPasswordService
{
    string Hash(string value);
    bool Verify(string value, string hash);
}

public class PasswordService : IPasswordService
{
    public string Hash(string value) => BCrypt.Net.BCrypt.HashPassword(value);
    public bool Verify(string value, string hash) => BCrypt.Net.BCrypt.Verify(value, hash);
}

public interface IJwtService
{
    string Create(User user);
}

public class JwtService(IConfiguration config) : IJwtService
{
    public string Create(User user)
    {
        var key = config["Jwt:Key"] ?? "dev-secret-key-change";
        var issuer = config["Jwt:Issuer"] ?? "spindle";
        var audience = config["Jwt:Audience"] ?? "spindle-client";
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            ],
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
