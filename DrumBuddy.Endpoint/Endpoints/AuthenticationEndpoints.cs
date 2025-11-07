using DrumBuddy.Endpoint.Models;
using DrumBuddy.Endpoint.Services;
using Microsoft.AspNetCore.Identity;

namespace DrumBuddy.Endpoint.Endpoints;


public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi();
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<User> userManager,
        TokenService tokenService)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Results.BadRequest(new { message = "User already exists" });
        }

        var user = new User
        {
            UserName = request.UserName ?? request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        var token = tokenService.GenerateAccessToken(user.Id, user.Email!);

        return Results.Ok(new
        {
            userId = user.Id,
            email = user.Email,
            token
        });
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        TokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        var token = tokenService.GenerateAccessToken(user.Id, user.Email!);

        return Results.Ok(new
        {
            userId = user.Id,
            email = user.Email,
            token
        });
    }
}