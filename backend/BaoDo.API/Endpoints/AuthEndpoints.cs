using BaoDo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BaoDo.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest req,
            IAuthService authService,
            CancellationToken ct) =>
        {
            var (token, user) = await authService.LoginAsync(req.Email, req.Password, ct);
            return Results.Ok(new { token, user });
        });

        group.MapPost("/register", async (
            [FromBody] RegisterRequest req,
            IAuthService authService,
            CancellationToken ct) =>
        {
            var (token, user) = await authService.RegisterAsync(req.Email, req.Password, req.FullName, ct);
            return Results.Ok(new { token, user });
        });

        group.MapGet("/profile", async (
            IAuthService authService,
            HttpContext context,
            CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            var profile = await authService.GetProfileAsync(userId, ct);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
        return Guid.Parse(claim.Value);
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FullName);
}
