using ManoMana.Application.Contracts;
using ManoMana.Application.Exceptions;
using ManoMana.Application.Interfaces.Repositories;
using ManoMana.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ManoMana.Application.Services;

public sealed class AuthenticationService(
    IAdminUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenIssuer tokenIssuer,
    ILogger<AuthenticationService> logger) : IAuthenticationService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            throw new UnauthorizedException("INVALID_CREDENTIALS", "Invalid username or password.");
        var user = await users.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Failed admin login attempt for username {Username}", request.Username.Trim());
            throw new UnauthorizedException("INVALID_CREDENTIALS", "Invalid username or password.");
        }
        return tokenIssuer.Issue(user);
    }
}
