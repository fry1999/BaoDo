using BaoDo.Core.Interfaces;
using BaoDo.Core.Models;
using BaoDo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BaoDo.Infrastructure.Services;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<(string token, UserProfile user)> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
            ?? throw new KeyNotFoundException("Email hoặc mật khẩu không đúng");

        if (user.Role == UserRole.Banned)
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị khóa");

        if (user.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new KeyNotFoundException("Email hoặc mật khẩu không đúng");

        var token = GenerateJwt(user);
        return (token, user);
    }

    public async Task<(string token, UserProfile user)> RegisterAsync(string email, string password, string fullName, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ArgumentException("Email đã được sử dụng");

        var user = new UserProfile
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = GenerateJwt(user);
        return (token, user);
    }

    public async Task<UserProfile?> GetProfileAsync(Guid userId, CancellationToken ct)
        => await db.Users.FindAsync([userId], ct);

    private string GenerateJwt(UserProfile user)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
