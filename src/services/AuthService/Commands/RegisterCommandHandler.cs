using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Services;
using MediatR;

namespace AuthService.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly JwtTokenService _jwt;

    public RegisterCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IRefreshTokenRepository refreshTokens,
        JwtTokenService jwt)
    {
        _users = users;
        _roles = roles;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var username = request.Username.Trim();
        var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("Email is already registered.");

        if (await _users.ExistsByUsernameAsync(username, cancellationToken))
            throw new InvalidOperationException("Username is already taken.");

        if (phoneNumber != null && await _users.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken))
            throw new InvalidOperationException("Phone number is already registered.");

        var role = await _roles.GetByNameAsync("User", cancellationToken)
            ?? throw new InvalidOperationException("Default role not found.");

        var user = new User
        {
            Username = username,
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.Id
        };

        await _users.CreateAsync(user, cancellationToken);

        var refreshTokenValue = _jwt.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            TokenHash = _jwt.HashRefreshToken(refreshTokenValue),
            UserId = user.Id,
            ExpiresAt = _jwt.GetRefreshTokenExpiry()
        };

        await _refreshTokens.CreateAsync(refreshToken, cancellationToken);

        return new AuthResponse(
            _jwt.Generate(user, role.Name),
            refreshTokenValue,
            new UserProfileDto(user.Id, user.Username, user.Email, user.PhoneNumber, role.Name));
    }

    private static string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }
}
