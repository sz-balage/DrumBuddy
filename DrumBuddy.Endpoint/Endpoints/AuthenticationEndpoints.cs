using DrumBuddy.Endpoint.Services;
using DrumBuddy.IO.Models;
using Microsoft.AspNetCore.Identity;

namespace DrumBuddy.Endpoint.Endpoints;


public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .AllowAnonymous();
        
        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi();
        
        group.MapPost("/forgot-password", ForgotPassword)
            .WithName("ForgotPassword")
            .WithOpenApi();
        
        group.MapPost("/reset-password", ResetPassword)
            .WithName("ResetPassword")
            .WithOpenApi();
    }

    private static async Task<IResult> Register(
        AuthRequests.RegisterRequest request,
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
        AuthRequests.LoginRequest request,
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
    private static async Task<IResult> ForgotPassword(
        AuthRequests.ForgotPasswordRequest request,
        UserManager<User> userManager,
        EmailService emailService,
        IConfiguration configuration)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            return Results.Ok(new { message = "If email exists, a reset link has been sent" });
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        
        var frontendUrl = configuration["AppSettings:FrontendUrl"];
        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";

        var emailSent = await emailService.SendPasswordResetEmailAsync(user.Email!, resetLink);

        if (!emailSent)
        {
            return Results.StatusCode(500);
        }

        return Results.Ok(new { message = "If email exists, a reset link has been sent" });
    }

    private static async Task<IResult> ResetPassword(
        AuthRequests.ResetPasswordRequest request,
        UserManager<User> userManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Results.BadRequest(new { message = "Invalid email" });
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Results.Ok(new { message = "Password reset successfully" });
    }
}