using DrumBuddy.Endpoint.Services;
using DrumBuddy.IO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
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
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await userManager.UpdateAsync(user);
        return Results.Ok(new
        {
            userName = user.UserName,
            email = user.Email,
            token,
            refreshToken,
            userId = user.Id
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
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        await userManager.UpdateAsync(user);

        return Results.Ok(new
        {
            userName = user.UserName,
            email = user.Email,
            token,
            refreshToken,
            userId = user.Id
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

        var websiteurl = configuration["AppSettings:WebsiteUrl"];
        var resetLink =
            $"{websiteurl}/reset-password.html?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";

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

    private static async Task<IResult> Refresh(
        AuthRequests.RefreshRequest request,
        UserManager<User> userManager,
        TokenService tokenService)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Results.Unauthorized();

        var newAccessToken = tokenService.GenerateAccessToken(user.Id, user.Email!);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

        await userManager.UpdateAsync(user);

        return Results.Ok(new AuthRequests.RefreshResponse(user.UserName, user.Email, newAccessToken, newRefreshToken,
            user.Id));
    }
}